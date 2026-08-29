namespace Drive;

public class GraphParseError
{
	public required string Message { get; init; }
}

public class GraphParseResult
{
	public required bool Success { get; init; }
	public Graph? Parsed { get; init; }
	public IReadOnlyList<GraphParseError>? Errors { get; init; }
}

public class Graph
{
	public IReadOnlyList<Partner> Partners { get; init; } = [];
	public IReadOnlyList<Company> Companies { get; init; } = [];
	public IReadOnlyList<Employee> Employees { get; init; } = [];
	public IReadOnlyList<Contact> Contacts { get; init; } = [];
	
	public static GraphParseResult Parse(ICollection<EntityDefinition> definitions)
	{
		var errors = new List<GraphParseError>();
		
		var partners = definitions
			.Where(x => x.Type == EntityType.Partner)
			.Select(x => new Partner(x.Parts[0]))
			.ToList();

		var companies = definitions
			.Where(x => x.Type == EntityType.Company)
			.Select(x => new Company(x.Parts[0]))
			.ToList();

		var employees = definitions
			.Where(x => x.Type == EntityType.Employee)
			.Select((x) =>
			{
				var company = companies.FirstOrDefault(c => c.Name == x.Parts[1]);

				if (company is null)
				{
					errors.Add(new GraphParseError { Message = $"Company {x.Parts[1]} not found." });
					return null;
				}

				return new Employee(x.Parts[0], company);
			})
			.ToList();

		var contacts = definitions
			.Where(x => x.Type == EntityType.Contact)
			.Select(x =>
			{
				var employee = employees.FirstOrDefault(e => e?.Name == x.Parts[0]);
				var partner = partners.FirstOrDefault(p => p.Name == x.Parts[1]);
				var contactTypeResult = Enum.TryParse(x.Parts[2], true, out ContactType contactType);

				if (employee == null)
					errors.Add(new GraphParseError { Message = $"Employee '{x.Parts[0]}' not found." });
				if (partner == null)
					errors.Add(new GraphParseError { Message = $"Partner '{x.Parts[1]}' not found." });
				if (!contactTypeResult)
					errors.Add(new GraphParseError { Message = $"Invalid ContactType provided '{x.Parts[1]}'. Must be one of (email, call, coffee)." });

				if (employee == null || partner == null || !contactTypeResult)
					return null;
				
				return new Contact(employee, partner, contactType);
			})
			.ToList();

		if (errors.Count > 0)
			return new GraphParseResult { Success = false, Errors = errors };

		return new GraphParseResult
		{
			Success = true,
			Parsed = new Graph
			{
				Partners = partners,
				Companies = companies,
				Employees = (IReadOnlyList<Employee>)employees,
				Contacts = (IReadOnlyList<Contact>)contacts
			}
		};
	}
}