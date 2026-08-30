using System.CommandLine;
using Drive;
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

    // Parse definitions into directed graph
    var graphResult = Graph.Parse(loadResult.Definitions);

    if (graphResult.Parsed is not null)
    {
        var graph = graphResult.Parsed;

        var contactGroups = graph.Contacts
            .GroupBy(x => (x.Employee.Company.Name, x.Partner.Name))
            .ToList();

        foreach (var c in graph.Companies.OrderBy(x => x.Value.Name))
        {
            var m = contactGroups
                .Where(x => x.Key.Item1 == c.Key)
                .MaxBy(x => x.Count());

            if (m is null)
            {
                Console.WriteLine($"{c.Key}: No current relationship");
            }
            else
            {
                Console.WriteLine($"{c.Key}: {m.Key.Item2} ({m.Count()})");
            }
        }
    }
    
    return 0;
});

return root.Parse(args).Invoke();
