using System.Data;
using System.Reflection;
using System.Xml.Linq;

namespace P21.Extensions.BusinessRule;

public class DataTableSerializer : IDataCollectionSerializer
{
    public DataTable ToDataTable(string xml)
    {
        XElement root = XDocument.Parse(xml).Root;
        var flag = false;
        var dt = new DataTable();
        var name = (XName)"fieldList";
        foreach (XElement descendant in root.Descendants(name))
        {
            if (!flag)
            {
                flag = InitializeDataTable(dt, descendant);
            }

            _ = dt.Rows.Add(ToRowData(descendant).ToArray());
        }
        return dt;
    }

    private IEnumerable<object> ToRowData(XElement xmlRow)
    {
        for (XNode currentNode = xmlRow.FirstNode; currentNode != null; currentNode = currentNode.NextNode)
        {
            if (currentNode is XElement xelement)
            {
                yield return xelement.Value;
            }
        }
    }

    private bool InitializeDataTable(DataTable dt, XElement xmlRow)
    {
        for (XNode xnode = xmlRow.FirstNode; xnode != null; xnode = xnode.NextNode)
        {
            if (xnode is XElement xelement)
            {
                _ = dt.Columns.Add(xelement.Name.ToString());
            }
        }
        return true;
    }

    public string ToXml(DataFields fields, string fileName, bool isFormRule)
    {
        var options = new XmlSerializationOptions()
        {
            IsFormRule = isFormRule,
            UseCompression = false
        };
        return ToXml(fields, fileName, options);
    }

    public string ToXml(DataFields fields, string fileName, XmlSerializationOptions options)
    {
        var xdocument = new XDocument(new XDeclaration("1.0", "utf-8", "no"),
        [
       new XElement((XName) "business_rule_extensions_xml")
        ]);
        PropertyInfo[] properties = typeof(DataField).GetProperties();
        foreach (DataField dataField in fields.OfType<DataField>())
        {
            var content1 = new XElement((XName)"fieldList");
            foreach (PropertyInfo propertyInfo in properties)
            {
                var name = ToCamelCase(propertyInfo.Name);
                var content2 = propertyInfo.GetValue(dataField, null);
                switch (name)
                {
                    case "triggerColumn":
                        name = "triggerField";
                        break;
                    case "columnName":
                    case "tableName":
                        continue;
                    case "modified":
                        name = "modifiedFlag";
                        content2 = dataField.Modified ? "Y" : "N";
                        break;
                }
                content1.Add(new XElement((XName)name, content2));
            }
            xdocument.Root.Add(content1);
        }
        return "<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>" + xdocument.ToString(SaveOptions.DisableFormatting);
    }

    private static string ToCamelCase(string str)
    {
        return char.ToLowerInvariant(str[0]).ToString() + str[1..];
    }
}
