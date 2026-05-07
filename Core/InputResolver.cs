namespace UnityCrackTool.Core;

public static class InputResolver
{
    public static IReadOnlyList<string> Resolve(IEnumerable<string>? dllPaths, string? directory)
    {
        var files = new List<string>();

        if (dllPaths is not null)
        {
            foreach (var entry in dllPaths)
                files.AddRange(ResolveGlob(entry));
        }

        if (directory is not null)
        {
            if (!Directory.Exists(directory))
                throw new DirectoryNotFoundException($"Directory not found: {directory}");
            files.AddRange(Directory.GetFiles(directory, "*.dll", SearchOption.AllDirectories));
        }

        return files.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static IEnumerable<string> ResolveGlob(string path)
    {
        if (File.Exists(path))
            return [path];

        bool recursive = path.Contains("**");
        string normalized = path.Replace("**" + Path.DirectorySeparatorChar, "")
                                .Replace("**/" , "")
                                .Replace("**\\", "");

        string dir = Path.GetDirectoryName(normalized) ?? ".";
        string pattern = Path.GetFileName(normalized);

        if (!Directory.Exists(dir))
            return [];

        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
        return Directory.GetFiles(dir, pattern, option);
    }
}
