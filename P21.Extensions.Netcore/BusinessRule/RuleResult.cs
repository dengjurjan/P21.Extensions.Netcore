namespace P21.Extensions.BusinessRule;

[Serializable]
public class RuleResult
{
    public static RuleResult SuccessfulResult => new();

    public bool Success { get; set; } = true;

    public string Message { get; set; } = "";

    public string? Keystroke { get; set; }

    public bool ShowResponse { get; set; }

    public ResponseAttributes? ResponseAttributes { get; set; }

    public string? ExternalUrl { get; set; }
}
