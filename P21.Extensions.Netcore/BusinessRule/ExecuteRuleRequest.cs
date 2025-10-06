using P21.Extensions.DataAccess;

namespace P21.Extensions.BusinessRule;

[Serializable]
public class ExecuteRuleRequest
{
    public string? RuleTypeName { get; set; }

    public string? CacheKey { get; set; }

    public RuleEntry? RuleEntry { get; set; }

    public string? XML { get; set; }

    public DBCredentials? DBCredentials { get; set; }

    public bool ExecuteAsync { get; set; }

    public string[]? PluginPaths { get; set; }
}
