using System.Text.RegularExpressions;

namespace Drive.Definitions;

public class DefinitionParser
{
	public Definition Parse(string row)
	{
		// At scale, this would be replaced with a pre-compiled regex
		var cleaned = Regex.Replace(row, @" +", " ");

		var parts = cleaned.Trim().Split(' ');
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