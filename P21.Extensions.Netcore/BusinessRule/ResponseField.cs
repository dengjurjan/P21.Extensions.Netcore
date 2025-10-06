namespace P21.Extensions.BusinessRule;

[Serializable]
public class ResponseField
{
    public ResponseField()
    {
    }

    public ResponseField(string name, string label, string dataType)
      : this(name, label, dataType, 0)
    {
    }

    public ResponseField(string name, string label, string dataType, int dataTypeLength)
      : this(name, label, dataType, dataTypeLength, string.Empty)
    {
    }

    private ResponseField(
      string name,
      string label,
      string dataType,
      int dataTypeLength,
      string dataValue)
    {
        Name = name;
        Label = label;
        DataType = dataType;
        DataTypeLength = dataTypeLength;
        DataValue = dataValue;
        switch (DataType)
        {
            case "char":
                break;
            case "long":
                break;
            case "decimal":
                break;
            case "datetime":
                break;
            case "checkbox":
                break;
            default:
                throw new ArgumentException("Invalid DataType for ResponseField: Supported values are ResponseFieldType.Alphanumeric, ResponseFieldType.Numeric, ResponseFieldType.Decimal, ResponseFieldType.DateTime, ResponseFieldType.Checkbox.", nameof(dataType));
        }
    }

    public void SetFieldValue<T>(T initialDataValue) => DataValue = initialDataValue.ToString();

    public string DataType { get; set; }

    public int DataTypeLength { get; set; }

    public string DataValue { get; set; }

    public string Label { get; set; }

    public string Name { get; set; }

    public string[] DropDownListDisplayValues { get; set; }

    public string[] DropDownListDataValues { get; set; }

    public bool ReadOnly { get; set; }

    public bool UsePasswordMask { get; set; }

    public int EditHeightScale { get; set; }

    public bool IsRequired { get; set; }
}
