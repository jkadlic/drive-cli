namespace Drive.Graphs;

public class Graph
{
	public required IReadOnlyDictionary<string, Partner> Partners { get; init; }
	public required IReadOnlyDictionary<string, Company> Companies { get; init; }
	public required IReadOnlyDictionary<string, Employee> Employees { get; init; }
	public required IReadOnlyList<Contact> Contacts { get; init; }
}