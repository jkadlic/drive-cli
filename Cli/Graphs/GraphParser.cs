using System.Diagnostics.CodeAnalysis;
using Drive.Definitions;

namespace Drive.Graphs;

public class GraphParseError
{
	public required string Error { get; init; }
	public required string Message { get; init; }
	
	public override string ToString() => $"{Error}: {Message}";
}

public class GraphParseResult
{
	[MemberNotNullWhen(true, nameof(Parsed))]
	[MemberNotNullWhen(false, nameof(Errors))]
	public required bool Success { get; init; }
	public Graph? Parsed { get; init; }
	public IReadOnlyList<GraphParseError>? Errors { get; init; }
}

public static class GraphParser
{
	private static int Rank(Definition definition) => definition switch
	{
		PartnerDefinition => 0,
		CompanyDefinition => 0,
		EmployeeDefinition => 1,
		ContactDefinition => 2,
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
			switch (def)
			{
				case PartnerDefinition partnerDef:
					partners[partnerDef.Name] = new Partner(partnerDef.Name);
					break;
				case CompanyDefinition companyDef:
					companies[companyDef.Name] = new Company(companyDef.Name);
					break;
				case EmployeeDefinition employeeDef:
					if (!companies.TryGetValue(employeeDef.CompanyName, out var company))
					{
						errors.Add(new GraphParseError { Error = $"Failed to parse Employee '{employeeDef.Name}'", Message = $"Company {employeeDef.CompanyName} not found" });
						break;
					}
					employees[employeeDef.Name] = new Employee(employeeDef.Name, company);
					break;
				case ContactDefinition contactDef:
					var employeeOk = employees.TryGetValue(contactDef.EmployeeName, out var employee);
					var partnerOk = partners.TryGetValue(contactDef.PartnerName, out var partner);
					var typeOk = Enum.TryParse<ContactType>(contactDef.ContactType, true, out var type);
					var description = $"{contactDef.EmployeeName}, {contactDef.PartnerName}, {contactDef.ContactType}";
					if (!employeeOk)
						errors.Add(new GraphParseError { Error = $"Failed to parse Contact '{description}'", Message = $"Employee {contactDef.EmployeeName} not found" });
					if (!partnerOk)
						errors.Add(new GraphParseError { Error = $"Failed to parse Contact '{description}'", Message = $"Partner {contactDef.PartnerName} not found" });
					if (!typeOk)
						errors.Add(new GraphParseError { Error = $"Failed to parse Contact '{description}'", Message = $"Invalid ContactType provided '{contactDef.ContactType}'. Must be one of (email, call, coffee)." });

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