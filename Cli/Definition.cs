namespace Drive;

public enum DefinitionType
{
	Partner,
	Company,
	Employee,
	Contact
}

public class Definition
{
	public required DefinitionType Type { get; init; }
	public required string[] Parts { get; init; }

	public static Definition Parse(string row)
	{
		var parts = row.Split(' ');
		var entityTypeResult = Enum.TryParse<DefinitionType>(parts[0], ignoreCase: true, out var type);
		if (!entityTypeResult)
			throw new ParseException($"Unknown definition type provided '{parts[0]}'. Must be one of (Partner, Company, Employee, Contact)");
		
		return new Definition
		{
			Type = type,
			Parts = parts.Skip(1).ToArray()
		};
	}
}