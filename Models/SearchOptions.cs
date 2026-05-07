using UnityCrackTool.Core;

namespace UnityCrackTool.Models;

public sealed record SearchOptions(
    IReadOnlyList<string> DllPaths,
    PatternMatcher ClassMatcher,
    PatternMatcher MethodMatcher,
    bool MatchFullName,
    bool IncludePrivate,
    bool IncludeNested
);
