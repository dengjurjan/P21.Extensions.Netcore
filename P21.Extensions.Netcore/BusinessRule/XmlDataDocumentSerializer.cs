using System.Data;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace P21.Extensions.BusinessRule;

public class XmlDataDocumentSerializer : IDataCollectionSerializer
{
    public DataTable ToDataTable(string xml)
    {
        var xmlDataDocument = new XmlDataDocument
        {
            XmlResolver = null
        };
        _ = xmlDataDocument.Schemas.Add(XmlSchema.Read(GetXsdAsStream(), null));
        xmlDataDocument.DataSet.ReadXmlSchema(GetXsdAsStream());
        xmlDataDocument.LoadXml(xml);
        xmlDataDocument.Validate(new ValidationEventHandler(ValidationEventHandler));
        return xmlDataDocument.DataSet.Tables["fieldList"];
    }

    private Stream GetXsdAsStream()
    {
        return new MemoryStream(Encoding.ASCII.GetBytes("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<xs:schema id=\"business_rule_extensions_xml\" xmlns=\"\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:msdata=\"urn:schemas-microsoft-com:xml-msdata\">\r\n<xs:element name=\"business_rule_extensions_xml\" msdata:IsDataSet=\"true\" msdata:UseCurrentLocale=\"true\">\r\n<xs:complexType>\r\n<xs:choice minOccurs=\"0\" maxOccurs=\"unbounded\">\r\n<xs:element name=\"fieldList\">\r\n<xs:complexType>\r\n<xs:sequence>\r\n<xs:element name=\"className\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"fieldTitle\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"fieldName\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"fieldAlias\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"fieldValue\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"modifiedFlag\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"readOnly\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"rowID\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"dataType\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"triggerField\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"triggerRow\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"fieldOriginalValue\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"updateSequence\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"setFocus\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"baseClassName\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"newRow\" type=\"xs:string\" minOccurs=\"0\" />\r\n<xs:element name=\"allowCascade\" type=\"xs:string\" minOccurs=\"0\" />\r\n</xs:sequence>\r\n</xs:complexType>\r\n</xs:element>\r\n</xs:choice>\r\n</xs:complexType>\r\n</xs:element>\r\n</xs:schema>"));
    }

    private void ValidationEventHandler(object sender, ValidationEventArgs e)
    {
        if (e.Exception != null)
        {
            throw e.Exception;
        }
    }

    private string BoolToYN(bool val) => !val ? "N" : "Y";

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
        if (options.IsFormRule)
        {
            var fieldValue = fields["file_path"].FieldValue;
            if (fieldValue.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || !File.Exists(fieldValue))
            {
                if (options.UseCompression)
                {
                    fileName = XMLDatastream.Compress(fileName);
                }

                fields["file_path"].FieldValue = fileName;
            }
        }
        var xmlDataDocument1 = new XmlDataDocument
        {
            XmlResolver = null
        };
        XmlDataDocument xmlDataDocument2 = xmlDataDocument1;
        _ = xmlDataDocument2.Schemas.Add(XmlSchema.Read(GetXsdAsStream(), null));
        xmlDataDocument2.DataSet.ReadXmlSchema(GetXsdAsStream());

        _ = xmlDataDocument2.DataSet.Tables["fieldList"];
        foreach (DataField field in fields)
        {
            _ = xmlDataDocument2.DataSet.Tables["fieldList"].Rows.Add(field.ClassName, field.FieldTitle, field.FieldName, field.FieldAlias, field.FieldValue, BoolToYN(field.Modified), field.ReadOnly, field.RowID, field.DataType, field.TriggerColumn, field.TriggerRow, field.FieldOriginalValue, field.UpdateSequence, field.SetFocus, field.BaseClassName, field.NewRow, field.AllowCascade);
        }

        return "<?xml version=\"1.0\" encoding=\"utf-16le\" standalone=\"no\"?>" + xmlDataDocument2.OuterXml;
    }
}
