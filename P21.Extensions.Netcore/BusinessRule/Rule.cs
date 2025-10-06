using Microsoft.Data.SqlClient;
using P21.Extensions.DataAccess;

namespace P21.Extensions.BusinessRule;

public abstract class Rule
{
    private DBCredentials? DbCredentials { get; set; }

    public void Initialize(DataCollection data, DBCredentials credentials)
    {
        Data = data;
        var logPath = Data.globals.GetValue("rule_code_log_path", "");
        Log = new LogString(GetType().ToString(), logPath);
        DbCredentials = credentials;
        var flag = Data.globals.TryGetValue("multirow", out var s) && s == "Y";
        var num = DbCredentials != null ? DbCredentials.cloudRestricted ? 1 : 0 : 0;
        var str1 = num != 0 ? "" : Data.globals.TryGetValue("global_server", out s) ? s : "";
        var str2 = num != 0 ? "" : Data.globals.TryGetValue("global_database", out s) ? s : "";
        Session = new Session()
        {
            UserID = Data.globals.GetValue("global_user_id", ""),
            Version = Data.globals.GetValue("global_version", ""),
            Server = str1,
            Database = str2,
            Language = Data.globals.GetValue("global_language", ""),
            MultiRow = flag,
            ID = Data.globals.GetValue("session_id", ""),
            ConfigurationID = Data.globals.GetValue("configuration_id", ""),
            RFLocationID = Data.globals.GetValue("rf_location_id", ""),
            RFBinID = Data.globals.GetValue("rf_bin_id", ""),
            ApplicationDisplayMode = Data.globals.GetValue("application_display_mode", ""),
            ClientPlatform = Data.globals.GetValue("client_platform", ""),
            WorkstationID = Data.globals.GetValue("workstation_id", ""),
            MiddlewareUrl = Data.globals.GetValue("middleware_url", ""),
            RuleCodeLogPath = logPath
        };
        RuleState = new RuleState()
        {
            UID = Data.ruleState.TryGetValue("uid", out s) ? int.Parse(s) : 0,
            Name = Data.ruleState.GetValue("name", ""),
            Type = Data.ruleState.GetValue("type", ""),
            ApplyOn = Data.ruleState.GetValue("apply_on", ""),
            MultiRow = Data.ruleState.TryGetValue("multirow_flag", out s) ? s == "Y" : flag,
            RunType = Data.ruleState.GetValue("run_type", ""),
            EventName = Data.ruleState.GetValue("event_name", ""),
            EventType = Data.ruleState.GetValue("event_type", ""),
            CascadeInProgress = Data.ruleState.TryGetValue("cascade_in_progress", out s) && s == "Y",
            AllowNewRows = Data.ruleState.TryGetValue("allow_new_rows", out s) && s == "Y",
            ThemeName = Data.ruleState.GetValue("theme_name", ""),
            TriggerWindowName = Data.ruleState.GetValue("trigger_window_name", ""),
            TriggerWindowTitle = Data.ruleState.GetValue("trigger_window_title", ""),
            RulePageUrl = Data.ruleState.GetValue("rule_page_url", ""),
            IsCallbackRule = Data.ruleState.TryGetValue("is_callback_rule", out s) && s == "Y",
            CallbackParentRule = Data.ruleState.GetValue("callback_parent_rule", ""),
            ConsumerKey = Data.ruleState.GetValue("consumer_key", ""),
            ConsumerName = Data.ruleState.GetValue("consumer_name", ""),
            RuleDataEntryMode = Enum.TryParse(Data.ruleState.GetValue("data_entry_mode", "0"), out RuleDataEntryMode result) ? result : RuleDataEntryMode.None
        };
    }

    public void Initialize(string xml, DBCredentials credentials)
    {
        Initialize(new DataCollection(xml), credentials);
        XmlData = xml;
    }

    public P21Connection? P21Connection { get; set; }

    private RulePopupService? rulePopupService { get; set; }

    public DataCollection? Data { get; set; }

    public LogString? Log { get; set; }

    public RuleState? RuleState { get; private set; }

    public Session? Session { get; private set; }

    public string? XmlData { get; private set; }

    public SqlConnection P21SqlConnection
    {
        get
        {
            if (P21Connection == null && DbCredentials != null)
            {
                P21Connection = ConnectionManager.GetP21Connection(DbCredentials, GetType().ToString());
            }

            return P21Connection.Connection;
        }
    }

    internal void CloseConnection()
    {
        if (P21Connection == null)
        {
            return;
        }

        ConnectionManager.CloseP21Connection(P21Connection);
    }

    public void CleanupConnection()
    {
        CloseConnection();
        P21Connection = null;
    }

    public RulePopupService RulePopupService
    {
        get
        {
            return rulePopupService ?? (rulePopupService = new RulePopupService(Session, RuleState));
        }
    }

    [Obsolete("Please use the [RuleDescription] attribute")]
    public virtual string GetName()
    {
        return GetAttribute<RuleDescriptionAttribute>()?.Name ?? GetType().Name;
    }

    public string Name => GetName();

    [Obsolete("Please use the [RuleDescription] attribute")]
    public virtual string GetDescription()
    {
        return GetAttribute<RuleDescriptionAttribute>()?.Description ?? GetType().FullName;
    }

    public string Description => GetDescription();

    private T GetAttribute<T>()
    {
        return GetType().GetCustomAttributes(typeof(T), false).Cast<T>().FirstOrDefault();
    }

    public abstract RuleResult Execute();

    public virtual RuleResultData Execute(ExecuteRuleRequest request)
    {
        Initialize(request.XML, request.DBCredentials);
        if (request.ExecuteAsync)
        {
            var thread = new Thread(() =>
            {
                try
                {
                    ExecuteAsync();
                }
                catch
                {
                }
            });
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            return new RuleResultData();
        }
        RuleResult ruleResult;
        try
        {
            ruleResult = Execute();
        }
        catch (Exception ex)
        {
            ruleResult = new RuleResult()
            {
                Success = false,
                Message = ex.Message
            };
        }
        var ruleResultData1 = new RuleResultData
        {
            Success = ruleResult.Success,
            Message = ruleResult.Message,
            Keystroke = ruleResult.Keystroke,
            ExternalUrl = ruleResult.ExternalUrl
        };
        RuleResultData ruleResultData2 = ruleResultData1;
        var str = (string)null;
        CloseConnection();
        if (ruleResult.GetType() == typeof(RuleResultData))
        {
            ruleResultData2.MessageTitleOverride = ((RuleResultData)ruleResult).MessageTitleOverride;
            str = ((RuleResultData)ruleResult).PredefinedReturnXml;
        }
        if (Data.UpdateByOrderCoded)
        {
            Data.OrderByUpdateSequence();
        }

        ruleResultData2.Xml = string.IsNullOrEmpty(str) ? Data.ToXml() : str;
        ruleResultData2.ShowResponse = ruleResult.ShowResponse;
        if (ruleResult.ResponseAttributes != null)
        {
            ruleResultData2.ResponseAttributes = ruleResult.ResponseAttributes;
        }

        ruleResultData2.Data = Data;
        return ruleResultData2;
    }

    [Obsolete("ExecuteAsync is deprecated, please use Execute method for synchronous and asynchronous rules.")]
    public virtual void ExecuteAsync()
    {
        try
        {
            _ = Execute();
        }
        catch (Exception ex)
        {
            Log.Add($"Failed to execute {GetType()} asynchronously: {ex.Message}");
        }
        finally
        {
            if (P21Connection != null)
            {
                ConnectionManager.CloseP21Connection(P21Connection);
            }
        }
    }

    [Obsolete("UnhiddeData is deprecated, please call RevealData")]
    protected string UnhiddeData(string originalvalue, string key)
    {
        return RevealData(originalvalue, key);
    }

    protected string RevealData(string originalvalue, string key)
    {
        return ConnectionManager.Decrypt(originalvalue, key);
    }

    protected string HideData(string originalvalue, string key)
    {
        return ConnectionManager.Encrypt(originalvalue, key);
    }
}
