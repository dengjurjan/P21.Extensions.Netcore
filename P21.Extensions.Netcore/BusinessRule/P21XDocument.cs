using System.Xml.Linq;
using System.Xml.Serialization;

namespace P21.Extensions.BusinessRule;

[XmlType(Namespace = "http://www.epicor.com/")]
[Serializable]
public class P21XDocument : XDocument
{
    public P21XDocument(XDocument document) : base(document)
    {
    }

    public new void Save(string filePath)
    {
        if (filePath.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || !File.Exists(filePath))
        {
            return;
        }

        base.Save(filePath);
    }
}
