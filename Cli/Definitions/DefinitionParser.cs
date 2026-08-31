using System.Text.RegularExpressions;

namespace Drive.Definitions;

internal enum DefinitionKeyword
{
	Partner,
	Company,
	Employee,
	Contact
}

public class DefinitionParser
{
	public Definition Parse(string row)
	{
		// At scale, this would be replaced with a pre-compiled regex
		var cleaned = Regex.Replace(row, @" +", " ");
		var parts = cleaned.Trim().Split(' ');

		var keyword = parts[0];
		var args = parts.Skip(1).ToArray();

		if (!Enum.TryParse<DefinitionKeyword>(keyword, ignoreCase: true, out var type))
			throw new ParseException($"Unknown definition type provided '{keyword}'. Must be one of (Partner, Company, Employee, Contact)");

		return type switch
		{
			DefinitionKeyword.Partner => new PartnerDefinition(RequireArgs(type, args, 1)[0]),
			DefinitionKeyword.Company => new CompanyDefinition(RequireArgs(type, args, 1)[0]),
			DefinitionKeyword.Employee => ParseEmployee(RequireArgs(type, args, 2)),
			DefinitionKeyword.Contact => ParseContact(RequireArgs(type, args, 3)),
			_ => throw new ParseException($"Unknown definition type provided '{keyword}'. Must be one of (Partner, Company, Employee, Contact)")
		};
	}

	private static EmployeeDefinition ParseEmployee(string[] args) => new(args[0], args[1]);

	private static ContactDefinition ParseContact(string[] args) => new(args[0], args[1], args[2]);

	private static string[] RequireArgs(DefinitionKeyword type, string[] args, int expected)
	{
		if (args.Length != expected)
			throw new ParseException($"'{type}' requires exactly {expected} argument(s), got {args.Length}.");

		return args;
	}
}
