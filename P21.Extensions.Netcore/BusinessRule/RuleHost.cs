using System.Reflection;

namespace P21.Extensions.BusinessRule;

public abstract class RuleHost : IDisposable
{
    protected List<string>? pluginPaths;
    private bool disposedValue;

    public List<string> PluginPaths => pluginPaths;

    public abstract List<RuleEntry> Rules { get; }

    public abstract RuleMetadataResult LoadRules(List<string> pluginPaths);

    public abstract RuleResultData ExecuteRule(ExecuteRuleRequest request);

    public abstract void UnloadRules();

    public virtual void LoadRulesFromAssembly(Assembly assembly)
    {
        throw new NotSupportedException("Host does not support loading from assemblies.");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposedValue)
        {
            return;
        }

        if (disposing)
        {
            UnloadRules();
        }

        disposedValue = true;
    }

    ~RuleHost() => Dispose(false);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
