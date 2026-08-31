namespace Drive.Definitions;

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
}