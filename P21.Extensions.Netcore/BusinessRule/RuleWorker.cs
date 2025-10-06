using System.Reflection;

namespace P21.Extensions.BusinessRule;

public class RuleWorker : MarshalByRefObject
{
    private string? _currentPath;
    private readonly Dictionary<string, Type> _types = [];
    private List<string> _pluginPaths = [];

    public bool UseLoadFile { get; set; }

    public List<RuleEntry> Rules { get; } = [];

    public void Init()
    {
        AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(domain_AssemblyResolve);
    }

    private Assembly domain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        _ = AssemblyLoadUtilities.TryLoadAssembly(args.Name, _currentPath, _pluginPaths?.ToArray(), out Assembly asm);
        return asm;
    }

    public RuleMetadataResult LoadRules(string[] pluginFolders)
    {
        _pluginPaths = _pluginPaths.Concat(pluginFolders).ToList();
        var messages = new List<string>();
        foreach (FileInfo assemblyFileInfo in pluginFolders.Distinct().SelectMany(f => GetDllsToSearch(f)))
        {
            AddRulesFromAssembly(assemblyFileInfo, messages);
        }

        return new RuleMetadataResult()
        {
            Rules = Rules,
            Messages = messages,
            PluginPaths = _pluginPaths.ToArray(),
            HasErrors = messages.Count > 0
        };
    }

    public void LoadRulesFromAssembly(Assembly assembly)
    {
        foreach (Type type in assembly.GetTypes().Where(t => !_types.TryGetValue(t.FullName, out Type _)))
        {
            if (TryCreateRuleEntry(type, out RuleEntry re))
            {
                _types.Add(type.FullName, type);
                Rules.Add(re);
            }
        }
    }

    private IEnumerable<FileInfo> GetSupportingFiles(Assembly a)
    {
        return new DirectoryInfo(_currentPath).GetFiles(a.ManifestModule.Name + ".*").Where(n => !n.Name.Equals(a.ManifestModule.Name));
    }

    private void AddRulesFromAssembly(FileInfo assemblyFileInfo, List<string> messages)
    {
        _currentPath = assemblyFileInfo.Directory.FullName;
        try
        {
            Assembly assembly = UseLoadFile ? Assembly.LoadFile(assemblyFileInfo.FullName) : Assembly.UnsafeLoadFrom(assemblyFileInfo.FullName);
            LoadRulesFromAssembly(assembly);
            var directoryName = Path.GetDirectoryName(assembly.Location);
            if (!(_currentPath != directoryName))
            {
                return;
            }

            foreach (FileInfo supportingFile in GetSupportingFiles(assembly))
            {
                var destFileName = Path.Combine(directoryName, supportingFile.Name);
                File.Copy(supportingFile.FullName, destFileName, true);
            }
        }
        catch (Exception ex)
        {
            messages.Add($"{assemblyFileInfo.FullName} - ({ex.GetType().Name}) {ex.Message}");
            if (!(ex is ReflectionTypeLoadException typeLoadException))
            {
                return;
            }

            foreach (Exception loaderException in typeLoadException.LoaderExceptions)
            {
                messages.Add($"{assemblyFileInfo.FullName} - ({loaderException.GetType().Name}) {loaderException.Message}");
            }
        }
    }

    private bool TryCreateRuleEntry(Type type, out RuleEntry? re)
    {
        if (!type.IsClass || !type.IsPublic || type.IsAbstract || !typeof(Rule).IsAssignableFrom(type))
        {
            re = null;
            return false;
        }
        var instance = (Rule)Activator.CreateInstance(type);
        re = new RuleEntry()
        {
            RuleTypeName = type.Name,
            RuleTypeFullName = type.FullName,
            RuleName = instance.Name,
            RuleDescription = instance.Description,
            AssemblyInfoName = type.Assembly.FullName,
            AssemblyPath = type.Assembly.Location,
            PrivateRule = type.GetCustomAttributes(typeof(PrivateRule), true).Any()
        };
        return true;
    }

    public bool TryCreateRule<T>(ExecuteRuleRequest request, out T? rule) where T : Rule
    {
        try
        {
            var ruleResultData = new RuleResultData();
            RuleEntry ruleEntry = Rules.Find(r => r.RuleTypeName.Equals(request.RuleTypeName));
            if (ruleEntry == null)
            {
                rule = default;
                return false;
            }
            _currentPath = new FileInfo(ruleEntry.AssemblyPath).DirectoryName;
            Type type = _types[ruleEntry.RuleTypeFullName];
            rule = (T)Activator.CreateInstance(type);
            return rule != null;
        }
        catch
        {
            rule = default;
            return false;
        }
    }

    public RuleResultData ExecuteRule(ExecuteRuleRequest request)
    {
        if (TryCreateRule(request, out Rule rule))
        {
            return rule.Execute(request);
        }

        var ruleResultData = new RuleResultData
        {
            Success = false,
            Message = $"Rule: {request.RuleTypeName} not found"
        };
        return ruleResultData;
    }

    private FileInfo[] GetDllsToSearch(string path)
    {
        if (!Directory.Exists(path))
        {
            return [];
        }

        return File.Exists(Path.Combine(path, "ruleDlls.txt")) ? File.ReadAllLines(Path.Combine(path, "ruleDlls.txt")).Where(dll => !string.IsNullOrEmpty(dll)).Select(dll => new FileInfo(Path.Combine(path, dll))).ToArray() : new DirectoryInfo(path).GetFiles("*.dll");
    }

    public override object InitializeLifetimeService() => null;
}
