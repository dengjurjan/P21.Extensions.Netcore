using System.Data;
using System.Globalization;
using System.Xml.Serialization;

namespace P21.Extensions.BusinessRule;

[XmlType(Namespace = "http://www.epicor.com/")]
[Serializable]
public class DataCollection
{
    internal Dictionary<string, string> globals = [];
    internal Dictionary<string, string> ruleState = [];
    private readonly DataSet set = new("P21Data");
    private int updateSequence;
    private readonly List<DataFieldKey> updateSequenceList = [];
    private bool multiRow;
    private readonly List<DataField> newRowDefinitions = [];
    private int nextNewRowID = -1;
    private bool allowNewRows;
    private bool formRule;
    private XMLDatastream xmlDatastream;

    public DataCollection(string xml) => PopulateData(xml);

    private DataFields _fields { get; set; } = [];

    public DataFields Fields
    {
        get
        {
            if (multiRow)
            {
                throw new BusinessRuleException("Data.Fields cannot be accessed in a multi-row rule.  Please use Data.Set or Session.");
            }

            if (formRule)
            {
                throw new BusinessRuleException("Data.Fields cannot be accessed in a form rule.  Please use Data.XMLDatastream or Session.");
            }

            return _fields;
        }
    }

    public DataSet Set
    {
        get
        {
            if (multiRow)
            {
                return set;
            }

            if (formRule)
            {
                throw new BusinessRuleException("Data.Set cannot be accessed in a form rule.  Please use Data.XMLDatastream.");
            }

            throw new BusinessRuleException("Data.Set cannot be accessed in a non-multi-row rule.  Please use Data.Fields.");
        }
    }

    public XMLDatastream XMLDatastream
    {
        get
        {
            if (formRule)
            {
                return xmlDatastream;
            }

            if (multiRow)
            {
                throw new BusinessRuleException("Data.DataStream cannot be accessed in a non-form rule.  Please use Data.Set.");
            }

            throw new BusinessRuleException("Data.DataStream cannot be accessed in a non-form rule.  Please use Data.Fields.");
        }
    }

    public string TriggerTable { get; private set; }

    public int TriggerRow { get; private set; }

    public string TriggerColumn { get; private set; }

    public string TriggerOriginalValue { get; private set; }

    public bool UpdateByOrderCoded { get; set; }

    private IDataCollectionSerializer GetSerializer()
    {
        return new XmlDataDocumentSerializer();
    }

    private void PopulateData(string xml)
    {
        var fields = new List<DataField>();
        var dataTable = GetSerializer().ToDataTable(xml);
        DataRow[] dataRowArray1 = dataTable.Select("className = 'rule_state' AND fieldName = 'multirow_flag'");
        if (dataRowArray1.Length == 1)
        {
            multiRow = Convert.ToString(dataRowArray1[0]["fieldValue"]) == "Y";
            UpdateByOrderCoded = true;
        }
        else
        {
            DataRow[] dataRowArray2 = dataTable.Select("className = 'global' AND fieldName = 'multirow'");
            if (dataRowArray2.Length == 1)
            {
                multiRow = Convert.ToString(dataRowArray2[0]["fieldValue"]) == "Y";
                UpdateByOrderCoded = true;
            }
        }
        DataRow[] dataRowArray3 = dataTable.Select("className = 'rule_state' AND fieldName = 'event_name'");
        if (dataRowArray3.Length == 1)
        {
            formRule = Convert.ToString(dataRowArray3[0]["fieldValue"]) == "Form Datastream Created";
        }

        DataRow[] dataRowArray4 = dataTable.Select("className = 'rule_state' AND fieldName = 'type'");
        var flag = false;
        if (dataRowArray4.Length == 1)
        {
            flag = Convert.ToString(dataRowArray4[0]["fieldValue"]) == "RMB";
        }

        updateSequence = dataTable.Rows.Count;
        foreach (DataRow row in (InternalDataCollectionBase)dataTable.Rows)
        {
            ++updateSequence;
            var field = new DataField(row["className"].ToString(), row["rowID"].ToString(), row["fieldTitle"].ToString(), row["fieldName"].ToString(), row["fieldAlias"].ToString(), row["dataType"].ToString(), row["fieldValue"].ToString(), row["readOnly"].ToString(), row["fieldOriginalValue"].ToString(), row["triggerField"].ToString(), row["triggerRow"].ToString(), updateSequence.ToString(), row["setFocus"].ToString(), row["baseClassName"].ToString(), row["newRow"].ToString(), row["allowCascade"].ToString());
            field.SetModifiedFlag(row["modifiedFlag"].ToString().ToUpper().Equals("Y"));
            fields.Add(field);
            if (multiRow && field.TableName != "global" && field.TableName != "rule_state")
            {
                AddFieldToSet(field);
            }

            if (field.TableName == "global" && !globals.TryGetValue(field.ColumnName, out var _))
            {
                globals.Add(field.ColumnName, field.FieldValue);
            }

            if (field.TableName == "rule_state" && !ruleState.TryGetValue(field.ColumnName, out var _))
            {
                ruleState.Add(field.ColumnName, field.FieldValue);
                if (field.FieldName == "allow_new_rows")
                {
                    allowNewRows = field.FieldValue == "Y";
                }
            }
            if (field.TriggerRow == "Y")
            {
                if (field.TriggerColumn == "Y" | flag)
                {
                    TriggerTable = field.TableName;
                    TriggerRow = Convert.ToInt32(field.RowID);
                }
                if (field.TriggerColumn == "Y")
                {
                    TriggerColumn = field.ColumnName;
                    TriggerOriginalValue = field.FieldOriginalValue;
                }
            }
        }
        _fields = new DataFields(fields);
        foreach (DataTable table in (InternalDataCollectionBase)set.Tables)
        {
            table.ColumnChanged += new DataColumnChangeEventHandler(DataSetColumnChanged);
        }

        set.AcceptChanges();
        if (!formRule)
        {
            return;
        }

        var fieldValue = _fields["file_path"].FieldValue;
        try
        {
            xmlDatastream = new XMLDatastream(fieldValue, IsDatastreamXmlCompressed(dataTable));
        }
        catch (Exception ex)
        {
            throw new BusinessRuleException("Error creating form data stream from " + fieldValue, ex);
        }
    }

    private void AddFieldToSet(DataField field)
    {
        Type dotNetType = ConvertPBToDotNetType(field.DataType);
        if (set.Tables[field.TableName] == null)
        {
            _ = set.Tables.Add(field.TableName);
            _ = set.Tables[field.TableName].Columns.Add("rowID", typeof(int));
            set.Tables[field.TableName].Columns["rowID"].ReadOnly = true;
        }
        if (set.Tables[field.TableName].Columns[field.ColumnName] == null)
        {
            _ = set.Tables[field.TableName].Columns.Add(field.ColumnName, dotNetType);
            set.Tables[field.TableName].Columns[field.ColumnName].Caption = field.FieldTitle;
            AddColumnToNewRowDefinition(field);
        }
        DataRow[] source = set.Tables[field.TableName].Select("rowID = " + field.RowID);
        if (!source.Any())
        {
            DataRow row = set.Tables[field.TableName].NewRow();
            row["rowID"] = Convert.ToInt32(field.RowID);
            if (field.FieldValue != "")
            {
                row[field.ColumnName] = ConvertFieldValueToType(field.FieldValue, dotNetType);
            }

            set.Tables[field.TableName].Rows.Add(row);
        }
        else
        {
            if (!(field.FieldValue.ToString() != ""))
            {
                return;
            }

            source[0][field.ColumnName] = ConvertFieldValueToType(field.FieldValue, dotNetType);
        }
    }

    private object ConvertFieldValueToType(string fieldValue, Type type)
    {
        return type == typeof(decimal) && fieldValue.IndexOf("e", StringComparison.InvariantCultureIgnoreCase) > -1 ? decimal.Parse(fieldValue, NumberStyles.Any, CultureInfo.InvariantCulture) : Convert.ChangeType(fieldValue, type);
    }

    private void AddColumnToNewRowDefinition(DataField field)
    {
        var className = field.ClassName;
        var num = 0;
        var rowID = num.ToString();
        var fieldTitle = field.FieldTitle;
        var fieldName = field.FieldName;
        var fieldAlias = field.FieldAlias;
        var dataType = field.DataType;
        var empty1 = string.Empty;
        var readOnly = field.ReadOnly;
        var empty2 = string.Empty;
        var triggerColumn = field.TriggerColumn;
        num = 0;
        var updateSequence = num.ToString();
        var setFocus = field.SetFocus;
        var baseClassName = field.BaseClassName;
        var allowCascade = field.AllowCascade;
        newRowDefinitions.Add(new DataField(className, rowID, fieldTitle, fieldName, fieldAlias, dataType, empty1, readOnly, empty2, triggerColumn, "N", updateSequence, setFocus, baseClassName, "Y", allowCascade));
    }

    public static Type ConvertPBToDotNetType(string pbType)
    {
        var str = pbType = pbType.ToUpper();
        if (str != null)
        {
            switch (str.Length)
            {
                case 3:
                    if (str == "INT")
                    {
                        goto label_17;
                    }

                    goto label_22;
                case 4:
                    switch (str[0])
                    {
                        case 'C':
                            if (str == "CHAR")
                            {
                                goto label_21;
                            }

                            goto label_22;
                        case 'D':
                            if (str == "DATE")
                            {
                                break;
                            }

                            goto label_22;
                        case 'L':
                            if (str == "LONG")
                            {
                                goto label_17;
                            }

                            goto label_22;
                        case 'R':
                            if (str == "REAL")
                            {
                                return typeof(float);
                            }

                            goto label_22;
                        case 'T':
                            if (str == "TIME")
                            {
                                break;
                            }

                            goto label_22;
                        default:
                            goto label_22;
                    }
                    break;
                case 5:
                    if (str == "ULONG")
                    {
                        return typeof(ulong);
                    }

                    goto label_22;
                case 6:
                    switch (str[0])
                    {
                        case 'N':
                            if (str == "NUMBER")
                            {
                                goto label_19;
                            }

                            goto label_22;
                        case 'S':
                            if (str == "STRING")
                            {
                                goto label_21;
                            }

                            goto label_22;
                        default:
                            goto label_22;
                    }
                case 7:
                    if (str == "DECIMAL")
                    {
                        goto label_19;
                    }

                    goto label_22;
                case 8:
                    if (str == "DATETIME")
                    {
                        break;
                    }

                    goto label_22;
                case 9:
                    if (str == "TIMESTAMP")
                    {
                        break;
                    }

                    goto label_22;
                default:
                    goto label_22;
            }
            return typeof(DateTime);
        label_17:
            return typeof(int);
        label_19:
            return typeof(decimal);
        label_21:
            return typeof(string);
        }
    label_22:
        if (pbType.Contains("CHAR"))
        {
            return typeof(string);
        }

        return pbType.Contains("DECIMAL") ? typeof(decimal) : typeof(string);
    }

    public string ToXml()
    {
        var serializationOptions = new XmlSerializationOptions
        {
            IsFormRule = formRule
        };
        XMLDatastream xmlDatastream = this.xmlDatastream;
        serializationOptions.UseCompression = xmlDatastream != null && xmlDatastream.UseCompression;
        XmlSerializationOptions options = serializationOptions;
        return GetSerializer().ToXml(_fields, this.xmlDatastream?.Document?.ToString() ?? "", options);
    }

    private void DataSetColumnChanged(object sender, DataColumnChangeEventArgs e)
    {
        IEnumerable<DataField> source = _fields.Cast<DataField>().Where(f => f.TableName == e.Row.Table.TableName && f.ColumnName == e.Column.ColumnName && f.RowID == e.Row["rowID"].ToString());
        if (source.Count() > 1)
        {
            e.Row.ClearErrors();
            e.Row.SetColumnError(e.Column.ColumnName, "Error - more than one field found with the given class name, row number, and field name.");
            e.Row.RejectChanges();
        }
        else
        {
            if (source.Count() != 1)
            {
                return;
            }

            DataField field = source.First();
            field.FieldValue = e.ProposedValue != null ? e.ProposedValue.ToString() : string.Empty;
            updateSequenceList.Add(new DataFieldKey(field));
        }
    }

    internal void RefreshGlobalFieldValue(string tableName, string fieldName, string fieldValue)
    {
        if (tableName != "rule_state" && tableName != "globals")
        {
            throw new InvalidOperationException("RefreshGlobalFieldValue method may only be used internally to refresh values for global or rule state _fields.");
        }

        IEnumerable<DataField> source = _fields.Cast<DataField>().Where(f => f.TableName == tableName && f.ColumnName == fieldName && f.RowID == string.Empty);
        if (source.Count() != 1)
        {
            return;
        }

        source.First().FieldValue = fieldValue;
    }

    internal void OrderByUpdateSequence()
    {
        var num1 = 0;
        foreach (DataFieldKey updateSequence in updateSequenceList)
        {
            DataFields fields = _fields;
            var tableName = updateSequence.TableName;
            var columnName = updateSequence.ColumnName;
            var num2 = updateSequence.RowID;
            var rowID = num2.ToString();
            DataField dataField = fields[tableName, columnName, rowID];
            num2 = num1++;
            var str = num2.ToString();
            dataField.UpdateSequence = str;
        }
    }

    private bool IsDatastreamXmlCompressed(DataTable dt)
    {
        var flag = false;
        if (dt != null)
        {
            DataRow[] dataRowArray = dt.Select("className = 'global' AND fieldName = 'compress_datastream_xml'");
            if (dataRowArray.Length == 1)
            {
                flag = Convert.ToString(dataRowArray[0]["fieldValue"]) == "Y";
            }
        }
        return flag;
    }

    public void SetFieldUpdateOrder(List<string> columns)
    {
        if (multiRow)
        {
            throw new InvalidOperationException("Cannot use SetFieldUpdateOrder with MultiRow rule.  Set Data.UpdateByOrderCoded instead.");
        }

        foreach (var column in columns)
        {
            updateSequenceList.Add(new DataFieldKey(_fields[column]));
        }

        UpdateByOrderCoded = true;
    }

    public DataFieldAttributes GetFieldAttributes(string tableName, string columnName, int rowID)
    {
        if (!multiRow)
        {
            throw new InvalidOperationException("GetFieldAttributes(string tableName, string columnName, int rowID) cannot be used with a single row rule.  Access field attributes directly via Fields.");
        }

        DataField field = _fields[tableName, columnName, rowID.ToString()];
        return new DataFieldAttributes(ref field);
    }

    public void SetFocus(string columnName)
    {
        if (multiRow)
        {
            throw new InvalidOperationException("SetFocus(String field) cannot be used with a multi-row rule.  Use SetFocus(int RowID, String field).");
        }

        _fields[columnName].SetFocus = "Y";
    }

    public void SetFocus(string columnName, int rowID)
    {
        if (!multiRow)
        {
            throw new InvalidOperationException("SetFocus(int RowID, String field) cannot be used with a single row rule.  Use SetFocus(String field).");
        }

        _fields[TriggerTable, columnName, rowID.ToString()].SetFocus = "Y";
    }

    public void SetFieldCascade(string columnName, bool allow)
    {
        if (multiRow)
        {
            throw new InvalidOperationException("SetFieldCascade(String field, bool allow) cannot be used with a multi-row rule.  Use SetCascade(String tableName, String columnName, int rowID, bool allow).");
        }

        _fields[columnName].AllowCascade = allow ? "Y" : "N";
    }

    public void SetCascade(bool allow)
    {
        foreach (DataField field in _fields)
        {
            field.AllowCascade = allow ? "Y" : "N";
        }
    }

    public void SetCascade(string tableName, bool allow)
    {
        if (!multiRow)
        {
            throw new BusinessRuleException("SetCascade(String tableName, bool allow) cannot be used with a single row rule.  Use SetFieldCascade(String field, bool allow).");
        }

        foreach (DataField dataField in _fields.Cast<DataField>().Where(f => f.TableName == tableName))
        {
            dataField.AllowCascade = allow ? "Y" : "N";
        }
    }

    public void SetCascade(string tableName, string columnName, bool allow)
    {
        if (!multiRow)
        {
            throw new BusinessRuleException(" SetCascade(String tableName, String columnName, bool allow) cannot be used with a single row rule.  Use SetFieldCascade(String field, bool allow).");
        }

        foreach (DataField dataField in _fields.Cast<DataField>().Where(f => f.TableName == tableName && f.ColumnName == columnName))
        {
            dataField.AllowCascade = allow ? "Y" : "N";
        }
    }

    public void SetCascade(string tableName, string columnName, int rowID, bool allow)
    {
        if (!multiRow)
        {
            throw new BusinessRuleException(" SetCascade(String tableName, String columnName, int rowID, bool allow) cannot be used with a single row rule.  Use SetFieldCascade(String field, bool allow).");
        }

        _fields[tableName, columnName, rowID.ToString()].AllowCascade = allow ? "Y" : "N";
    }

    public DataRow AddNewRow(string tableName)
    {
        try
        {
            if (!allowNewRows)
            {
                throw new BusinessRuleException($"You are not allowed to add new rows to table {tableName}.");
            }

            DataTable table = set.Tables[tableName];
            DataRow row = table.NewRow();
            row["rowID"] = nextNewRowID--;
            table.Rows.Add(row);
            foreach (DataField dataField in newRowDefinitions.Cast<DataField>().Where(t => t.TableName == tableName))
            {
                ++updateSequence;
                _fields.Add(new DataField(dataField.ClassName, row["rowID"].ToString(), dataField.FieldTitle, dataField.FieldName, dataField.FieldAlias, dataField.DataType, dataField.FieldValue, dataField.ReadOnly, dataField.FieldOriginalValue, dataField.TriggerColumn, dataField.TriggerRow, updateSequence.ToString(), dataField.SetFocus, dataField.BaseClassName, dataField.NewRow, dataField.AllowCascade));
            }
            return row;
        }
        catch (Exception ex)
        {
            throw new BusinessRuleException($"Cannot add new row to table{tableName}.", ex);
        }
    }

    public DataRow AddNewRow(DataTable table) => AddNewRow(table.TableName);

    public bool IsTriggerTable(string tableName) => TriggerTable == tableName;

    public bool IsTriggerRow(string tableName, int rowID)
    {
        return TriggerTable == tableName && TriggerRow == rowID;
    }

    public bool IsTriggerField(string tableName, int rowID, string columnName)
    {
        return TriggerTable == tableName && TriggerRow == rowID && TriggerColumn == columnName;
    }

    public int GetActiveRowIDForTable(string tableName)
    {
        if (!Set.Tables.Contains("table_properties"))
        {
            return -1;
        }

        DataRow[] dataRowArray = Set.Tables["table_properties"].Select($"table = '{tableName}'");
        if (dataRowArray.Length != 1)
        {
            return -1;
        }

        _ = int.TryParse(dataRowArray[0]["active_row"].ToString(), out var result);
        return result;
    }

    public DataRow GetActiveRowForTable(string tableName)
    {
        var activeRowForTable = (DataRow)null;
        if (!Set.Tables.Contains("table_properties") || !Set.Tables.Contains(tableName))
        {
            return activeRowForTable;
        }

        var activeRowIdForTable = GetActiveRowIDForTable(tableName);
        if (activeRowIdForTable == -1)
        {
            return activeRowForTable;
        }

        DataRow[] dataRowArray = Set.Tables[tableName].Select("rowID = " + activeRowIdForTable.ToString());
        if (dataRowArray.Length == 1)
        {
            activeRowForTable = dataRowArray[0];
        }

        return activeRowForTable;
    }
}
