using System.Data;

namespace P21.Extensions.BusinessRule;

public interface IDataCollectionSerializer
{
    DataTable ToDataTable(string xml);

    string ToXml(DataFields fields, string fileName, bool isFormRule);

    string ToXml(DataFields fields, string fileName, XmlSerializationOptions options);
}
