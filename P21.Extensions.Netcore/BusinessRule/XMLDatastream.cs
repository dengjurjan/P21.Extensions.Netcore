using System.IO.Compression;
using System.Text;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace P21.Extensions.BusinessRule;

[XmlType(Namespace = "http://www.epicor.com/")]
[Serializable]
public class XMLDatastream
{
    public XMLDatastream(string filePath)
      : this(filePath, false)
    {
    }

    public XMLDatastream(string filePath, bool useCompression)
    {
        XDocument document;
        if (IsValidFileName(filePath))
        {
            document = XDocument.Load(filePath, LoadOptions.None);
        }
        else
        {
            if (useCompression)
            {
                filePath = Decompress(filePath);
            }

            document = XDocument.Parse(filePath, LoadOptions.None);
        }
        Document = new P21XDocument(document);
        FilePath = filePath;
        UseCompression = useCompression;
    }

    public string FilePath { get; }

    public XDocument Document { get; }

    public bool UseCompression { get; }

    public IEnumerable<XElement> GetForms()
    {
        return Document.Descendants((XName)"FORMXXXDEF").Select(form => form);
    }

    public IEnumerable<XElement> GetHeaders()
    {
        return Document.Descendants((XName)"HDRXXXXDEF").Select(head => head);
    }

    public XElement GetHeader(XElement form) => form.Element((XName)"HDRXXXXDEF");

    public IEnumerable<XElement> GetLines() => GetLines(Document.Root);

    public IEnumerable<XElement> GetLines(XElement form)
    {
        return form.Descendants((XName)"LINEXXXDEF").Where(line => (string)line.Attribute((XName)"lineKey") == null);
    }

    public IEnumerable<XElement> GetLinesBy(string elementName, string elementValue)
    {
        return GetLinesBy(Document.Root, elementName, elementValue);
    }

    public IEnumerable<XElement> GetLinesBy(XElement form, string elementName, string elementValue)
    {
        return form.Descendants((XName)"LINEXXXDEF").Where(line => line.Elements((XName)elementName).Where(child => child.Value == elementValue).Any() && (string)line.Attribute((XName)"lineKey") == null);
    }

    public IEnumerable<XElement> GetTotals()
    {
        return Document.Descendants((XName)"TOTALSXDEF").Select(total => total);
    }

    public XElement GetTotal(XElement form) => form.Element((XName)"TOTALSXDEF");

    public IEnumerable<XElement> GetGroup(string groupName, XElement fromElement)
    {
        if (!(fromElement.Name == (XName)"LINEXXXDEF") || (string)fromElement.Attribute((XName)"lineKey") != null)
        {
            return fromElement.Elements((XName)groupName);
        }

        var lineKey = fromElement.Attribute((XName)"key").Value;
        return Document.Descendants((XName)"LINEXXXDEF").Where(e => (string)e.Attribute((XName)"lineKey") == lineKey).Elements((XName)groupName);
    }

    public void AddGroup(XElement groupToAdd, XElement addToElement)
    {
        if ((string)addToElement.Attribute((XName)"key") == null)
        {
            throw new BusinessRuleException($"Cannot add group to {addToElement.Name} as it does not contain a 'key' attribute.");
        }

        var content1 = new XAttribute((XName)"type", "0");
        var content2 = new XAttribute((XName)"typeName", "Generic");
        groupToAdd.Add(content1);
        groupToAdd.Add(content2);
        var content3 = new XAttribute((XName)"key", Guid.NewGuid().ToString("B"));
        groupToAdd.Add(content3);
        if (addToElement.Name == (XName)"LINEXXXDEF" && (string)addToElement.Attribute((XName)"lineKey") == null)
        {
            var content4 = new XElement((XName)"LINEXXXDEF");
            var content5 = new XAttribute((XName)"parentKey", addToElement.Parent.Attribute((XName)"key").Value);
            var lineKey = new XAttribute((XName)"lineKey", addToElement.Attribute((XName)"key").Value);
            var content6 = new XAttribute((XName)"originalGroupName", "LINEXXXDEF");
            var content7 = new XAttribute((XName)"type", "6");
            var content8 = new XAttribute((XName)"typeName", "Line");
            var str = Guid.NewGuid().ToString("B");
            var content9 = new XAttribute((XName)"key", str);
            var content10 = new XAttribute((XName)"parentKey", str);
            content4.Add(content7);
            content4.Add(content8);
            content4.Add(content9);
            content4.Add(content5);
            content4.Add(lineKey);
            content4.Add(content6);
            groupToAdd.Add(content10);
            groupToAdd.Add(lineKey);
            content4.Add(groupToAdd);
            Document.Descendants((XName)"LINEXXXDEF").Where(e => (string)e.Attribute((XName)"lineKey") == lineKey.Value).Last().AddAfterSelf(content4);
        }
        else
        {
            var content11 = new XAttribute((XName)"parentKey", addToElement.Attribute((XName)"key").Value);
            groupToAdd.Add(content11);
            addToElement.Add(groupToAdd);
        }
    }

    public void SortLines(string sortElement, bool numeric = false, bool descending = false)
    {
        foreach (XElement form in GetForms().ToList())
        {
            SortLines(form, sortElement, numeric, descending);
        }
    }

    public void SortLines(XElement form, string sortElement, bool numeric = false, bool descending = false)
    {
        if (form.Name != (XName)"FORMXXXDEF")
        {
            throw new BusinessRuleException("Cannot sort form by lines. The form parameter is not a FORMXXXDEF element.");
        }

        if (form.Element((XName)"LINEXXXDEF").Element((XName)sortElement) == null)
        {
            throw new BusinessRuleException($"Cannot sort form by lines. LINEXXXDEF does not contain element {sortElement}.");
        }

        try
        {
            List<XElement> xelementList = !numeric ? !descending ? GetLines(form).OrderBy(line => line.Element((XName)sortElement).Value).ToList() : GetLines(form).OrderByDescending(line => line.Element((XName)sortElement).Value).ToList() : !descending ? GetLines(form).OrderBy(line => Convert.ToDecimal(line.Element((XName)sortElement).Value)).ToList() : GetLines(form).OrderByDescending(line => Convert.ToDecimal(line.Element((XName)sortElement).Value)).ToList();
            var source = new List<XElement>();
            foreach (XElement xelement in xelementList)
            {
                var lineKey = xelement.Attribute((XName)"key").Value;
                source.AddRange(form.Elements((XName)"LINEXXXDEF").Where(e => e.Attribute((XName)"lineKey")?.Value == lineKey));
            }
            foreach (XNode xnode in form.Elements((XName)"LINEXXXDEF").ToList())
            {
                xnode.Remove();
            }

            xelementList.Reverse();
            source.Reverse();
            XElement header = GetHeader(form);
            foreach (XElement xelement in xelementList)
            {
                XElement line = xelement;
                foreach (XElement content in source.Where(group => group.Attribute((XName)"lineKey").Value == line.Attribute((XName)"key").Value))
                {
                    header.AddAfterSelf(content);
                }

                header.AddAfterSelf(line);
            }
        }
        catch (Exception ex)
        {
            throw new BusinessRuleException("Cannot sort form by lines.", ex);
        }
    }

    public void SortLineGroup(string groupName, string sortElement, bool numeric = false, bool descending = false)
    {
        foreach (XElement form in GetForms().ToList())
        {
            foreach (XElement line in GetLines(form).ToList())
            {
                SortLineGroup(form, line, groupName, sortElement, numeric, descending);
            }
        }
    }

    public void SortLineGroup(
      XElement form,
      string groupName,
      string sortElement,
      bool numeric = false,
      bool descending = false)
    {
        foreach (XElement line in GetLines(form).ToList())
        {
            SortLineGroup(form, line, groupName, sortElement, numeric, descending);
        }
    }

    public void SortLineGroup(
      XElement form,
      XElement line,
      string groupName,
      string sortElement,
      bool numeric = false,
      bool descending = false)
    {
        if (form.Name != (XName)"FORMXXXDEF")
        {
            throw new BusinessRuleException("Cannot sort form by group. Form parameter is not a FORMXXXDEF element.");
        }

        try
        {
            var lineKey = line.Attribute((XName)"key").Value;
            IEnumerable<XElement> source = form.Elements((XName)"LINEXXXDEF").Where(e => e.Attribute((XName)"lineKey")?.Value == lineKey && e.Elements().FirstOrDefault()?.Name == (XName)groupName);
            List<XElement> xelementList = !numeric ? !descending ? source.OrderBy(group => group.Element((XName)groupName).Element((XName)sortElement).Value).ToList() : source.OrderByDescending(group => group.Element((XName)groupName).Element((XName)sortElement).Value).ToList() : !descending ? source.OrderBy(group => Convert.ToDecimal(group.Element((XName)groupName).Element((XName)sortElement).Value)).ToList() : source.OrderByDescending(group => Convert.ToDecimal(group.Element((XName)groupName).Element((XName)sortElement).Value)).ToList();
            foreach (XNode xnode in source.ToList())
            {
                xnode.Remove();
            }

            xelementList.Reverse();
            foreach (XElement content in xelementList)
            {
                line.AddAfterSelf(content);
            }
        }
        catch (Exception ex)
        {
            throw new BusinessRuleException("Cannot sort form by group.", ex);
        }
    }

    public static bool IsValidFileName(string filename)
    {
        return filename.IndexOfAny(Path.GetInvalidPathChars()) < 0 && File.Exists(filename);
    }

    public static string Compress(string datastreamXml)
    {
        string base64String;
        using (var memoryStream = new MemoryStream())
        {
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
            {
                using (var streamWriter = new StreamWriter(gzipStream))
                {
                    streamWriter.Write(datastreamXml);
                }
            }
            base64String = Convert.ToBase64String(memoryStream.ToArray());
        }
        return string.IsNullOrWhiteSpace(base64String) ? datastreamXml : base64String;
    }

    public static string Decompress(string base64String)
    {
        using (var memoryStream = new MemoryStream(Convert.FromBase64String(base64String)))
        {
            using (var gzipStream = new GZipStream(memoryStream, CompressionMode.Decompress))
            {
                using (var streamReader = new StreamReader(gzipStream, Encoding.UTF8))
                {
                    return streamReader.ReadToEnd();
                }
            }
        }
    }
}
