using System.Xml.Serialization;

namespace P21.Extensions.BusinessRule;

[XmlType(Namespace = "http://www.epicor.com/")]
[Serializable]
public class RuleState
{
    public RuleState()
    {
    }

    [Obsolete("Use the parameterless constructor")]
    public RuleState(
      string uid,
      string name,
      string type,
      string applyOn,
      bool multiRow,
      string runType,
      string eventName,
      string eventType,
      bool cascadeInProgress,
      bool allowNewRows,
      string themeName,
      string triggerWindowName,
      string triggerWindowTitle,
      string rulePageUrl,
      bool isCallbackRule,
      string callbackParentRule)
    {
        UID = Convert.ToInt32(uid);
        Name = name;
        Type = type;
        ApplyOn = applyOn;
        MultiRow = multiRow;
        RunType = runType;
        EventName = eventName;
        EventType = eventType;
        CascadeInProgress = cascadeInProgress;
        AllowNewRows = allowNewRows;
        ThemeName = themeName;
        TriggerWindowName = triggerWindowName;
        TriggerWindowTitle = triggerWindowTitle;
        RulePageUrl = rulePageUrl;
        IsCallbackRule = isCallbackRule;
        CallbackParentRule = callbackParentRule;
    }

    public int UID { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }

    public string ApplyOn { get; set; }

    public bool MultiRow { get; set; }

    public string RunType { get; set; }

    public string EventName { get; set; }

    public string EventType { get; set; }

    public bool CascadeInProgress { get; set; }

    public string ThemeName { get; set; }

    public string TriggerWindowName { get; set; }

    public string TriggerWindowTitle { get; set; }

    [Obsolete("RuleState.ApplicationDisplayMode is obsolete, please use Session.ApplicationDisplayMode instead.")]
    public string ApplicationDisplayMode { get; private set; }

    [Obsolete("RuleState.ClientPlatform is obsolete, please use Session.ClientPlatform instead.")]
    public string ClientPlatform { get; private set; }

    public string RulePageUrl { get; set; }

    public bool IsCallbackRule { get; set; }

    public string CallbackParentRule { get; set; }

    public bool AllowNewRows { get; set; }

    public string ConsumerKey { get; set; }

    public string ConsumerName { get; set; }

    public RuleDataEntryMode RuleDataEntryMode { get; set; }
}
