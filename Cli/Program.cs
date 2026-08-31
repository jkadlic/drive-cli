using System.CommandLine;
using Drive.Graphs;
using Drive.Definitions;

var fileOption = new Option<FileInfo>("--file")
{
    Description = "The file to read entity definitions from."
};

var root = new RootCommand("analyze");
root.Options.Add(fileOption);

root.SetAction(result =>
{
    var file = result.GetValue(fileOption);
    if (file is null)
    {
        Console.Error.WriteLine("Error: No --file provided.");
        return 1;
    }

    if (!file.Exists)
    {
        Console.Error.WriteLine($"Error: File not found: {file.FullName}");
        return 1;
    }
    
    // Load definitions from file and check for errors
    var loadResult = DefinitionLoader.LoadFromFile(file.FullName);
    if (!loadResult.Success)
    {
        Console.Error.WriteLine("Failed to read definitions:\n");
        foreach (var e in loadResult.Errors)
            Console.Error.WriteLine(e);
        return 1;
    }

    // Parse definitions into directed graph and check for errors
    var graphResult = GraphParser.Parse(loadResult.Definitions);
    if (!graphResult.Success)
    {
        Console.Error.WriteLine("Failed to parse definitions into graph:\n");
        foreach (var e in graphResult.Errors)
            Console.Error.WriteLine(e);
        return 1;
    }

    var graph = graphResult.Parsed;
    
    var analysis = new CompanyRelationshipAnalyzer().Analyze(graph);
    Console.WriteLine(analysis.ToString());
    
    return 0;
});

return root.Parse(args).Invoke();
