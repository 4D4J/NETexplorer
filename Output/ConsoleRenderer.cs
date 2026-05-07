using UnityCrackTool.Models;

namespace UnityCrackTool.Output;

public static class ConsoleRenderer
{
    public static int Render(IEnumerable<(string AssemblyPath, IReadOnlyList<ClassMatch> Matches)> results)
    {
        int totalClasses = 0;
        int totalAssemblies = 0;

        foreach (var (path, classes) in results)
        {
            totalAssemblies++;
            totalClasses += classes.Count;

            Write($"[{path}]", ConsoleColor.Cyan);

            foreach (var cls in classes)
            {
                string header = "  " + cls.ClassName;
                if (cls.BaseTypeName is not null)
                    header += $" : {cls.BaseTypeName}";
                Write(header, ConsoleColor.Yellow);

                foreach (var method in cls.Methods)
                {
                    Write($"    {cls.ClassName}.{method.Name}", ConsoleColor.White);
                    Write($"      {method.Signature}", ConsoleColor.Green);
                }
            }
            Console.WriteLine();
        }

        if (totalClasses == 0)
        {
            Console.WriteLine("No matches found.");
            return 1;
        }

        string asmWord = totalAssemblies == 1 ? "assembly" : "assemblies";
        Console.WriteLine($"Found {totalClasses} class(es) across {totalAssemblies} {asmWord}.");
        return 0;
    }

    private static void Write(string text, ConsoleColor color)
    {
        Console.ForegroundColor = color;
        try { Console.WriteLine(text); }
        finally { Console.ResetColor(); }
    }
}
