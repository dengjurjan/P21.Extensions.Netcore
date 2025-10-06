namespace P21.Extensions.BusinessRule;

[Serializable]
public class RuleMetadataResult
{
    public string[]? PluginPaths { get; set; }

    public List<RuleEntry>? Rules { get; set; }

    public List<string>? Messages { get; set; }

    public bool HasErrors { get; set; }

    public string? CacheKey { get; set; }
}
