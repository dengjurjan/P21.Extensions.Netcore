using System.Collections;
using System.Xml.Serialization;

namespace P21.Extensions.BusinessRule;

[XmlType(Namespace = "http://www.epicor.com/")]
[Serializable]
public class DataUpdateSequence : IEnumerable
{
    private readonly List<DataFieldKey>? updateSequence;

    public DataFieldKey this[int index] => updateSequence[index];

    public DataFieldKey this[string tableName, string columnName, int rowID]
    {
        get
        {
            return updateSequence.Find(k => k.TableName.Equals(tableName, StringComparison.InvariantCultureIgnoreCase) && k.ColumnName.Equals(columnName, StringComparison.InvariantCultureIgnoreCase) && k.RowID == rowID) ?? throw new InvalidOperationException($"Update sequence not found for table: {tableName}, column: {columnName}, row: {rowID}.");
        }
    }

    public IEnumerator GetEnumerator()
    {
        return new DataFieldKeyEnumerator(updateSequence.ToArray());
    }
}
