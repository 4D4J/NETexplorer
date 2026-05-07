using dnlib.DotNet;
using UnityCrackTool.Models;

namespace UnityCrackTool.Core;

public static class AssemblyScanner
{
    public static IEnumerable<(string AssemblyPath, IReadOnlyList<ClassMatch> Matches)> Scan(SearchOptions options)
    {
        foreach (var path in options.DllPaths)
        {
            IReadOnlyList<ClassMatch>? matches = null;
            try
            {
                matches = ScanFile(path, options);
            }
            catch (BadImageFormatException)
            {
                
            }
            catch (IOException ex)
            {
                Console.Error.WriteLine($"[warn] Could not read {path}: {ex.Message}");
            }

            if (matches is not null && matches.Count > 0)
                yield return (path, matches);
        }
    }

    private static IReadOnlyList<ClassMatch> ScanFile(string path, SearchOptions options)
    {
        using var module = ModuleDefMD.Load(path);

        IEnumerable<TypeDef> types = options.IncludeNested
            ? module.GetTypes()
            : (IEnumerable<TypeDef>)module.Types;

        var results = new List<ClassMatch>();

        foreach (var type in types)
        {
            string typeName = type.Name.String;
            if (typeName.StartsWith('<')) continue;

            string matchTarget = options.MatchFullName ? type.FullName : typeName;
            if (!options.ClassMatcher.IsMatch(matchTarget)) continue;

            var methods = new List<MethodMatch>();
            foreach (var method in type.Methods)
            {
                if (method.Name.String.StartsWith('<')) continue;
                if (!options.IncludePrivate && method.IsPrivate) continue;
                if (!options.MethodMatcher.IsMatch(method.Name.String)) continue;

                methods.Add(new MethodMatch(method.Name.String, SignatureFormatter.Format(method)));
            }

            if (methods.Count > 0 || options.MethodMatcher.IsMatch(""))
                results.Add(new ClassMatch(typeName, GetBaseName(type), methods));
        }
        return results;
    }

    private static string? GetBaseName(TypeDef type)
    {
        string? baseName = type.BaseType?.Name;
        return baseName is null or "Object" ? null : baseName;
    }
}
