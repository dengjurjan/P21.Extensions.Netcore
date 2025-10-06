using System.Xml.Serialization;

namespace P21.Extensions.BusinessRule;

[XmlType(Namespace = "http://www.epicor.com/")]
[Serializable]
public class DataField
{
    private readonly string className;
    private readonly string fieldTitle;
    private readonly string fieldName;
    private readonly string fieldAlias;
    private string fieldValue;
    private bool modified;
    private readonly string rowID;
    private readonly string readOnly;
    private readonly string dataType;
    private readonly string fieldOriginalValue;
    private readonly string triggerColumn;
    private readonly string triggerRow;
    private string updateSequence;
    private string setFocus;
    private readonly string baseClassName;
    private readonly string tableName;
    private readonly string columnName;
    private string newRow;
    private string allowCascade;

    public DataField(
      string className,
      string rowID,
      string fieldTitle,
      string fieldName,
      string fieldAlias,
      string dataType,
      string fieldValue,
      string readOnly,
      string fieldOriginalValue,
      string triggerColumn,
      string triggerRow,
      string updateSequence,
      string setFocus,
      string baseClassName,
      string newRow,
      string allowCascade)
    {
        this.className = className;
        this.rowID = rowID;
        this.fieldTitle = fieldTitle;
        this.fieldName = fieldName;
        this.fieldAlias = fieldAlias;
        this.dataType = dataType;
        this.fieldValue = fieldValue;
        this.readOnly = readOnly;
        this.fieldOriginalValue = fieldOriginalValue;
        this.triggerColumn = triggerColumn;
        this.triggerRow = triggerRow;
        this.updateSequence = updateSequence;
        this.setFocus = setFocus;
        this.baseClassName = baseClassName;
        tableName = baseClassName == "" ? className : baseClassName;
        columnName = fieldAlias == "" ? fieldName : fieldAlias;
        this.newRow = newRow;
        this.allowCascade = allowCascade;
    }

    public string ClassName => className;

    public string FieldTitle => fieldTitle;

    public string FieldName => fieldName;

    public string FieldAlias => fieldAlias;

    public string FieldValue
    {
        get => fieldValue;
        set
        {
            fieldValue = value;
            modified = true;
        }
    }

    public bool Modified => modified;

    internal void SetModifiedFlag(bool value) => modified = value;

    public string RowID => rowID;

    public string ReadOnly => readOnly;

    public string DataType => dataType;

    public string FieldOriginalValue => fieldOriginalValue;

    public string TriggerColumn => triggerColumn;

    public string TriggerRow => triggerRow;

    public string UpdateSequence
    {
        get => updateSequence;
        internal set => updateSequence = value;
    }

    public string SetFocus
    {
        get => setFocus;
        internal set => setFocus = value;
    }

    public string BaseClassName => baseClassName;

    public string TableName => tableName;

    public string ColumnName => columnName;

    public string NewRow
    {
        get => newRow;
        internal set => newRow = value;
    }

    public string AllowCascade
    {
        get => allowCascade;
        internal set => allowCascade = value;
    }
}
