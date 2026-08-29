using System.CommandLine;
using Drive;

var fileOption = new Option<FileInfo>("--file")
{
    Description = "The file to read entity definitions from."
};

var root = new RootCommand("DriveCli");
root.Options.Add(fileOption);

root.SetAction(result =>
{
    var file = result.GetValue(fileOption);
    if (file == null) return 1;
    
    var rows = File.ReadAllLines(file.FullName);
    var defs = rows.Select(EntityDefinition.Parse).ToList();

    var graphResult = Graph.Parse(defs);

    if (graphResult.Errors is { Count: > 0 })
    {
        foreach (var e in graphResult.Errors)
        {
            Console.WriteLine($"Error -> {e.Message}");
        }    
    }

    if (graphResult.Parsed is not null)
    {
        foreach (var x in graphResult.Parsed.Partners)
        {
            Console.WriteLine($"Partner -> {x.Name}");
        }
        
        foreach (var x in graphResult.Parsed.Companies)
        {
            Console.WriteLine($"Company -> {x.Name}");
        }
        
        foreach (var x in graphResult.Parsed.Employees)
        {
            Console.WriteLine($"Employees -> {x.Name} @ {x.Company.Name}");
        }
        
        foreach (var x in graphResult.Parsed.Contacts)
        {
            Console.WriteLine($"Contact -> {x.Partner.Name} : {x.Employee.Name} via {x.Type}");
        }
    }


    return 0;
});

return root.Parse(args).Invoke();























