using Epicor.P21.Popup.Model.Launcher;
using System.Reflection;

namespace P21.Extensions.BusinessRule;

public class RulePopupService
{
    public static Type? PopupLauncherType;
    private readonly IPopupLauncher _launcher;

    public RulePopupService(Session session, RuleState ruleState)
    {
        _launcher = CreatePopupLauncher();
        _launcher.Init(new SimplePopupSettings()
        {
            Database = session.Database,
            Server = session.Server,
            ThemeName = ruleState.ThemeName
        });
    }

    private static IPopupLauncher CreatePopupLauncher()
    {
        if (PopupLauncherType == null)
        {
            PopupLauncherType = FindType("Epicor.P21.Popup.Client.StandardPopupLauncher", "Epicor.P21.Popup.Client");
        }

        return Activator.CreateInstance(PopupLauncherType) is IPopupLauncher instance ? instance : throw new TypeLoadException("Epicor.P21.Popup.Client.StandardPopupLauncher found but is not castable to IPopupLauncher");
    }

    private static Type FindType(string typeName, string assembly)
    {
        var type1 = Type.GetType(typeName);
        if (type1 != null)
        {
            return type1;
        }

        foreach (Assembly assembly1 in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type2 = assembly1.GetType(typeName);
            if (type2 != null)
            {
                return type2;
            }
        }
        var path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assembly + ".dll");
        if (!File.Exists(path))
        {
            throw new TypeLoadException($"Assembly {assembly} not found at {path}");
        }

        try
        {
            return Assembly.LoadFile(path).GetType(typeName);
        }
        catch (Exception ex)
        {
            throw new TypeLoadException($"Unable to find type {typeName} at {path}", ex);
        }
    }

    public string ShowPopup(string fieldName) => _launcher.LaunchPopup(fieldName);

    public string ShowPopup(string fieldName, string additionalWhereClause)
    {
        return _launcher.LaunchPopup(fieldName, additionalWhereClause);
    }
}
