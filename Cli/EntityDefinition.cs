namespace Drive;

public enum EntityType
{
	Partner,
	Company,
	Employee,
	Contact
}

public class EntityDefinition
{
	public EntityType Type { get; init; }
	public string[] Parts { get; init; }

	public static EntityDefinition Parse(string row)
	{
		var parts = row.Split(' ');
		var entityTypeResult = Enum.TryParse<EntityType>(parts[0], ignoreCase: true, out var type);
		if (!entityTypeResult)
			throw new ParseException($"Unknown definition type provided '{parts[0]}'. Must be one of (Partner, Company, Employee, Contact)");
		
		return new EntityDefinition
		{
			Type = type,
			Parts = parts.Skip(1).ToArray()
		};
	}
}