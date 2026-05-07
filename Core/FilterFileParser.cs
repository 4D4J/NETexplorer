namespace UnityCrackTool.Core;

public sealed record FilterFile(IReadOnlyList<string> ClassPatterns, IReadOnlyList<string> MethodPatterns);

public static class FilterFileParser
{
    public static FilterFile Parse(string path)
    {
        var lines = File.ReadAllLines(path);
        var classPatterns  = new List<string>();
        var methodPatterns = new List<string>();

        List<string>? current = null;

        foreach (var raw in lines)
        {
            var line = raw.TrimEnd();
            if (line.TrimStart().StartsWith('#')) continue;
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.TrimStart() == "class:")  { current = classPatterns;  continue; }
            if (line.TrimStart() == "method:") { current = methodPatterns; continue; }

            if (current is not null && line.TrimStart().StartsWith("- "))
                current.Add(line.TrimStart()[2..].Trim());
        }
        return new FilterFile(classPatterns, methodPatterns);
    }
}
