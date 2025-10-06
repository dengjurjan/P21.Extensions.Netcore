namespace P21.Extensions.BusinessRule;

[AttributeUsage(AttributeTargets.Class)]
[Serializable]
public sealed class RuleDescriptionAttribute : Attribute
{
    public string Name { get; }

    public string Description { get; }

    public RuleDescriptionAttribute(string description) => Description = description;

    public RuleDescriptionAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
