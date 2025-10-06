namespace P21.Extensions.BusinessRule;

[Serializable]
public class RuleResultData : RuleResult
{
    public bool ContinueAfterPreRun { get; set; } = true;

    public string? Xml { get; set; }

    public DataCollection? Data { get; set; }

    public string? MessageTitleOverride { get; set; }

    public string? PredefinedReturnXml { get; set; }
}
