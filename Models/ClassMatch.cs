namespace UnityCrackTool.Models;

public sealed record ClassMatch(
    string ClassName,
    string? BaseTypeName,
    IReadOnlyList<MethodMatch> Methods
);
