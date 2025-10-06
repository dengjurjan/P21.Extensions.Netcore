using System.Xml.Serialization;

namespace P21.Extensions.BusinessRule;

[XmlType(Namespace = "http://www.epicor.com/")]
[Serializable]
public class DataFieldKey
{
    public string TableName { get; private set; }

    public string ColumnName { get; private set; }

    public int RowID { get; private set; }

    public DataFieldKey(DataField field)
    {
        TableName = field.TableName;
        ColumnName = field.ColumnName;
        if (!(field.RowID != ""))
        {
            return;
        }

        RowID = Convert.ToInt32(field.RowID);
    }
}
