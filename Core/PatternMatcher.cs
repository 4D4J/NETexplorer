using System.Text.RegularExpressions;

namespace UnityCrackTool.Core;

public sealed class PatternMatcher
{
    private readonly Regex? _regex;

    private PatternMatcher(Regex? regex) => _regex = regex;

    public bool IsMatch(string input) => _regex is null || _regex.IsMatch(input);

    public static PatternMatcher Any() => new(null);

    public static PatternMatcher FromWildcard(string pattern)
    {
        string rxPattern = "^" + Regex.Escape(pattern)
            .Replace(@"\*", ".*")
            .Replace(@"\?", ".") + "$";
        return new PatternMatcher(new Regex(rxPattern,
            RegexOptions.IgnoreCase | RegexOptions.Compiled));
    }

    public static PatternMatcher FromWildcards(IEnumerable<string> patterns)
    {
        var list = patterns.ToList();
        if (list.Count == 0) return Any();
        if (list.Count == 1) return FromWildcard(list[0]);

        string combined = string.Join("|", list.Select(p =>
            "(?:" + Regex.Escape(p).Replace(@"\*", ".*").Replace(@"\?", ".") + ")"));
        return new PatternMatcher(new Regex(
            $"^(?:{combined})$",
            RegexOptions.IgnoreCase | RegexOptions.Compiled));
    }

    public static PatternMatcher FromRegex(string pattern)
    {
        try
        {
            return new PatternMatcher(new Regex(pattern,
                RegexOptions.Compiled));
        }
        catch (ArgumentException ex)
        {
            throw new ArgumentException($"Invalid regex pattern '{pattern}': {ex.Message}", ex);
        }
    }
}
