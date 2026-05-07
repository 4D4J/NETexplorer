using System.CommandLine;
using UnityCrackTool.Commands;

var root = new RootCommand("Search for classes and methods in .NET assemblies using dnlib");
SearchCommand.Configure(root);
return await root.InvokeAsync(args);
