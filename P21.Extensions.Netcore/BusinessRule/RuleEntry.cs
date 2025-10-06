namespace P21.Extensions.BusinessRule;

[Serializable]
public class RuleEntry
{
    public Type? RuleType { get; set; }

    public string? RuleTypeName { get; set; }

    public string? RuleTypeFullName { get; set; }

    public string? RuleName { get; set; }

    public string? RuleDescription { get; set; }

    public string? AssemblyInfoName { get; set; }

    public string? AssemblyPath { get; set; }

    public bool PrivateRule { get; set; }
}
