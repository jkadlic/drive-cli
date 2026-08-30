using System.CommandLine;
using Drive;

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
    
    var lines = File.ReadAllLines(file.FullName);
    var defs = new List<Definition>();
    var readErrors = new List<string>();

    for (var i = 0; i < lines.Length; i++)
    {
        if (string.IsNullOrWhiteSpace(lines[i]))
            continue;

        try
        {
            defs.Add(Definition.Parse(lines[i]));
        }
        catch (ParseException ex)
        {
            readErrors.Add($"Line {i + 1}: {ex.Message}");
        }
    }

    if (readErrors.Count > 0)
    {
        Console.Error.WriteLine("Failed to read definitions:\n");
        foreach (var e in readErrors)
            Console.Error.WriteLine(e);
        return 1;
    }

    var graphResult = Graph.Parse(defs);

    if (graphResult.Parsed is not null)
    {
        var graph = graphResult.Parsed;

        var contactGroups = graph.Contacts
            .GroupBy(x => (x.Employee.Company.Name, x.Partner.Name));

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
