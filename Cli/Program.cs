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
    if (file == null) return 1;
    
    var rows = File.ReadAllLines(file.FullName);
    var defs = rows.Select(Definition.Parse).ToList();

    var graphResult = Graph.Parse(defs);


    if (graphResult.Errors is { Count: > 0 })
    {
        Console.WriteLine("Graph parsing failed with the following errors:\n");
        
        foreach (var e in graphResult.Errors)
        {
            Console.WriteLine($"{e.Error} : {e.Message}");
        }    
    }

    if (graphResult.Parsed is not null)
    {
        var graph = graphResult.Parsed;
        foreach (var x in graph.Partners.Keys)
        {
            Console.WriteLine($"Partner -> {graph.Partners[x].Name}");
        }
        
        foreach (var x in graph.Companies.Keys)
        {
            Console.WriteLine($"Company -> {graph.Companies[x].Name}");
        }
        
        foreach (var x in graph.Employees.Keys)
        {
            Console.WriteLine($"Employees -> {graph.Employees[x].Name} @ {graph.Employees[x].Company.Name}");
        }
        
        foreach (var x in graph.Contacts)
        {
            Console.WriteLine($"Contact -> {x.Partner.Name} : {x.Employee.Name} via {x.Type}");
        }
    }


    return 0;
});

return root.Parse(args).Invoke();























