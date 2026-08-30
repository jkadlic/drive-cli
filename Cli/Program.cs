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

        var groups = graph.Contacts
            .GroupBy(x => x.Employee.Company.Name)
            .Select(x =>
            {
                var partner = x
                    .GroupBy(y => y.Partner.Name)
                    .OrderBy(y => y.Count())
                    .Select(y => y.Key)
                    .First();
                
                return new
                {
                    Company = x.Key,
                    Count = x.Count(),
                    Partner = partner
                };
            })
            .ToList();

        foreach (var g in groups)
        {
            var ret = (g.Count > 0)
                ? $"{g.Company}: {g.Partner} ({g.Count})"
                : $"{g.Company}: No current relationship";
            Console.WriteLine(ret);
        }
    }
    
    return 0;
});

return root.Parse(args).Invoke();
