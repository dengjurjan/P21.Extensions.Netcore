using System.Reflection;

namespace P21.Extensions;

internal static class AssemblyLoadUtilities
{
    internal static bool TryLoadAssembly(
      string assemblyName,
      string currentPath,
      string[] searchPaths,
      out Assembly? asm)
    {
        asm = null;
        var path2 = $"{assemblyName[..assemblyName.IndexOf(",")]}.dll";
        var str = Path.Combine(currentPath, path2);
        var flag = File.Exists(str);
        if (!flag)
        {
            foreach (var searchPath in searchPaths)
            {
                if (!searchPath.Equals(currentPath, StringComparison.CurrentCultureIgnoreCase))
                {
                    str = Path.Combine(searchPath, path2);
                    flag = File.Exists(str);
                    if (flag)
                    {
                        break;
                    }
                }
            }
        }
        if (flag && File.Exists(str))
        {
            asm = Assembly.LoadFrom(str);
        }

        return asm != null;
    }
}
