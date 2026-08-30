using System.Diagnostics.CodeAnalysis;
using Drive.Definitions;

namespace Drive;

public class GraphParseError
{
	public required string Error { get; init; }
	public required string Message { get; init; }
}

public class GraphParseResult
{
	[MemberNotNullWhen(true, nameof(Parsed))]
	[MemberNotNullWhen(false, nameof(Errors))]
	public required bool Success { get; init; }
	public Graph? Parsed { get; init; }
	public IReadOnlyList<GraphParseError>? Errors { get; init; }
}

public class Graph
{
	public required IReadOnlyDictionary<string, Partner> Partners { get; init; }
	public required IReadOnlyDictionary<string, Company> Companies { get; init; }
	public required IReadOnlyDictionary<string, Employee> Employees { get; init; }
	public required IReadOnlyList<Contact> Contacts { get; init; }

	private static int Rank(Definition definition) => definition.Type switch
	{
		DefinitionType.Partner => 0,
		DefinitionType.Company => 0,
		DefinitionType.Employee => 1,
		DefinitionType.Contact => 2,
		_ => int.MaxValue
	};
	
	public static GraphParseResult Parse(ICollection<Definition> definitions)
	{
		var errors = new List<GraphParseError>();
		var partners = new Dictionary<string, Partner>();
		var companies = new Dictionary<string, Company>();
		var employees = new Dictionary<string, Employee>();
		var contacts = new List<Contact>();

		foreach (var def in definitions.OrderBy(Rank))
		{
			switch (def.Type)
			{
				case DefinitionType.Partner:
					partners[def.Parts[0]] = new Partner(def.Parts[0]);
					break;
				case DefinitionType.Company:
					companies[def.Parts[0]] = new Company(def.Parts[0]);
					break;
				case DefinitionType.Employee:
					if (!companies.TryGetValue(def.Parts[1], out var company))
					{
						errors.Add(new GraphParseError { Error = $"Failed to parse Employee '{def.Parts[0]}'", Message = $"Company {def.Parts[1]} not found" });
						break;
					}
					employees[def.Parts[0]] = new Employee(def.Parts[0], company);
					break;
				case DefinitionType.Contact:
					var employeeOk = employees.TryGetValue(def.Parts[0], out var employee);
					var partnerOk = partners.TryGetValue(def.Parts[1], out var partner);
					var typeOk = Enum.TryParse<ContactType>(def.Parts[2], true, out var type);
					if (!employeeOk)
						errors.Add(new GraphParseError { Error = $"Failed to parse Contact '{string.Join(", ", def.Parts)}'", Message = $"Employee {def.Parts[0]} not found" });
					if (!partnerOk)
						errors.Add(new GraphParseError { Error = $"Failed to parse Contact '{string.Join(", ", def.Parts)}'", Message = $"Partner {def.Parts[1]} not found" });
					if (!typeOk)
						errors.Add(new GraphParseError { Error = $"Failed to parse Contact '{string.Join(", ", def.Parts)}'", Message = $"Invalid ContactType provided '{def.Parts[2]}'. Must be one of (email, call, coffee)." });
					
					if (employeeOk && partnerOk && typeOk)
						contacts.Add(new Contact(employee!, partner!, type));
					break;
			}
		}

		if (errors.Count > 0)
			return new GraphParseResult { Success = false, Errors = errors };

		return new GraphParseResult
		{
			Success = true,
			Parsed = new Graph
			{
				Partners = partners,
				Companies = companies,
				Employees = employees,
				Contacts = contacts
			}
		};
	}
}