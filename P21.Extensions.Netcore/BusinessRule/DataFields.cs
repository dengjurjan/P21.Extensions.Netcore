using System.Collections;
using System.Xml.Serialization;

namespace P21.Extensions.BusinessRule;

[XmlType(Namespace = "http://www.epicor.com/")]
[Serializable]
public class DataFields : IEnumerable<DataField>, IEnumerable
{
    private readonly List<DataField> _fields;

    public DataFields()
      : this([])
    {
    }

    public DataFields(List<DataField> fields) => _fields = fields;

    internal void Add(DataField field) => _fields.Add(field);

    public DataField this[int index] => _fields[index];

    public DataField this[string name]
    {
        get
        {
            return _fields.Find(f => string.Equals(f.FieldName, name, StringComparison.CurrentCultureIgnoreCase)) ?? throw new KeyNotFoundException($"Field name {name} not found.");
        }
    }

    public DataField this[string tableName, string columnName, string rowID]
    {
        get
        {
            return _fields.Find(f => string.Equals(f.TableName, tableName, StringComparison.CurrentCultureIgnoreCase) && string.Equals(f.ColumnName, columnName, StringComparison.CurrentCultureIgnoreCase) && f.RowID == rowID) ?? throw new KeyNotFoundException($"Field with table: {tableName}, column: {columnName}, row: {rowID} not found.");
        }
    }

    public DataField GetFieldByAlias(string alias)
    {
        return _fields.Find(f => string.Equals(f.FieldAlias, alias, StringComparison.CurrentCultureIgnoreCase)) ?? throw new KeyNotFoundException($"Field alias {alias} not found.");
    }

    IEnumerator<DataField> IEnumerable<DataField>.GetEnumerator()
    {
        return _fields.GetEnumerator();
    }

    public IEnumerator GetEnumerator() => _fields.GetEnumerator();
}
