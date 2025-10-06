using System.Xml.Serialization;

namespace P21.Extensions.BusinessRule;

[XmlType(Namespace = "http://www.epicor.com/")]
[Serializable]
public class DataFieldAttributes
{
    public DataFieldAttributes(ref DataField field)
    {
        Key = new DataFieldKey(field);
        _dataField = field;
    }

    private DataField _dataField { get; set; }

    public DataFieldKey Key { get; private set; }

    public string Title => _dataField.FieldTitle;

    public string Name => _dataField.FieldName;

    public string Alias => _dataField.FieldAlias;

    public string DataType => _dataField.DataType;

    public bool ReadOnly => _dataField.ReadOnly == "Y";

    public bool AllowCascade => _dataField.AllowCascade == "Y";
}
