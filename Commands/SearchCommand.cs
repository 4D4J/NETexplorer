using System.CommandLine;
using UnityCrackTool.Core;
using UnityCrackTool.Models;
using UnityCrackTool.Output;

namespace UnityCrackTool.Commands;

public static class SearchCommand
{
    public static void Configure(RootCommand cmd)
    {

        var dllOpt = new Option<string[]?>(
            "--dll",
            "DLL path(s) or glob pattern(s), e.g. C:/Managed/*.dll")
        {
            AllowMultipleArgumentsPerToken = true,
            Arity = ArgumentArity.ZeroOrMore
        };

        var dirOpt    = new Option<string?>("--dir",            "Scan directory recursively for *.dll files");
        var filterOpt = new Option<string?>("--filter-file",    "YAML-like .txt file with class:/method: pattern lists");
        var clsOpt    = new Option<string?>("--class",          "Wildcard pattern for class name (* ?)");
        var methOpt   = new Option<string?>("--method",         "Wildcard pattern for method name (* ?)");
        var clsRxOpt  = new Option<string?>("--class-regex",    "Regex pattern for class name");
        var methRxOpt = new Option<string?>("--method-regex",   "Regex pattern for method name");
        var fullOpt   = new Option<bool>   ("--full-name",      "Match patterns against fully-qualified class name");
        var privOpt   = new Option<bool>   ("--include-private","Include non-public methods");
        var noNestOpt = new Option<bool>   ("--no-nested",      "Skip nested types");

        cmd.AddOption(filterOpt);
        cmd.AddOption(dllOpt);
        cmd.AddOption(dirOpt);
        cmd.AddOption(clsOpt);
        cmd.AddOption(methOpt);
        cmd.AddOption(clsRxOpt);
        cmd.AddOption(methRxOpt);
        cmd.AddOption(fullOpt);
        cmd.AddOption(privOpt);
        cmd.AddOption(noNestOpt);

        cmd.SetHandler(ctx =>
        {
            var pr = ctx.ParseResult;
            var filter         = pr.GetValueForOption(filterOpt);
            var dll            = pr.GetValueForOption(dllOpt);
            var dir            = pr.GetValueForOption(dirOpt);
            var cls            = pr.GetValueForOption(clsOpt);
            var meth           = pr.GetValueForOption(methOpt);
            var clsRx          = pr.GetValueForOption(clsRxOpt);
            var methRx         = pr.GetValueForOption(methRxOpt);
            var fullName       = pr.GetValueForOption(fullOpt);
            var includePrivate = pr.GetValueForOption(privOpt);
            var noNested       = pr.GetValueForOption(noNestOpt);

            if (filter is not null && (cls is not null || clsRx is not null || meth is not null || methRx is not null))
            {
                Console.Error.WriteLine("Error: --filter-file cannot be combined with --class / --class-regex / --method / --method-regex.");
                ctx.ExitCode = 2;
                return;
            }

            if (cls is not null && clsRx is not null)
            {
                Console.Error.WriteLine("Error: --class and --class-regex are mutually exclusive.");
                ctx.ExitCode = 2;
                return;
            }
            if (meth is not null && methRx is not null)
            {
                Console.Error.WriteLine("Error: --method and --method-regex are mutually exclusive.");
                ctx.ExitCode = 2;
                return;
            }

            IReadOnlyList<string> paths;
            try
            {
                paths = InputResolver.Resolve(dll, dir);
            }
            catch (DirectoryNotFoundException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                ctx.ExitCode = 2;
                return;
            }

            if (paths.Count == 0)
            {
                Console.Error.WriteLine("Error: no DLL files found. Check --dll / --dir arguments.");
                ctx.ExitCode = 2;
                return;
            }

            PatternMatcher classMatcher;
            PatternMatcher methodMatcher;
            try
            {
                if (filter is not null)
                {
                    var ff = FilterFileParser.Parse(filter);
                    classMatcher  = ff.ClassPatterns.Count  > 0 ? PatternMatcher.FromWildcards(ff.ClassPatterns)  : PatternMatcher.Any();
                    methodMatcher = ff.MethodPatterns.Count > 0 ? PatternMatcher.FromWildcards(ff.MethodPatterns) : PatternMatcher.Any();
                }
                else
                {
                    classMatcher  = cls   is not null ? PatternMatcher.FromWildcard(cls)
                                  : clsRx is not null ? PatternMatcher.FromRegex(clsRx)
                                  : PatternMatcher.Any();

                    methodMatcher = meth   is not null ? PatternMatcher.FromWildcard(meth)
                                  : methRx is not null ? PatternMatcher.FromRegex(methRx)
                                  : PatternMatcher.Any();
                }
            }
            catch (ArgumentException ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                ctx.ExitCode = 2;
                return;
            }

            var options = new SearchOptions(
                DllPaths:       paths,
                ClassMatcher:   classMatcher,
                MethodMatcher:  methodMatcher,
                MatchFullName:  fullName,
                IncludePrivate: includePrivate,
                IncludeNested:  !noNested
            );

            var results = AssemblyScanner.Scan(options);
            ctx.ExitCode = ConsoleRenderer.Render(results);
        });
    }
}
