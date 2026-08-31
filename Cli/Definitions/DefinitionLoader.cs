using System.Diagnostics.CodeAnalysis;

namespace Drive.Definitions;

public record DefinitionLoadResult
{
	[MemberNotNullWhen(true, nameof(Definitions))]
	[MemberNotNullWhen(false, nameof(Errors))]
	public required bool Success { get; init; }
	public List<Definition>? Definitions { get; init; }
	public List<string>? Errors { get; init; }
}

public static class DefinitionLoader
{
	public static DefinitionLoadResult LoadFromFile(string filePath)
	{
		var lines = File.ReadAllLines(filePath);
		var defs = new List<Definition>();
		var errors = new List<string>();

		var parser = new DefinitionParser();
		for (var i = 0; i < lines.Length; i++)
		{
			if (string.IsNullOrWhiteSpace(lines[i]))
				continue;

			try
			{
				defs.Add(parser.Parse(lines[i]));
			}
			catch (ParseException ex)
			{
				errors.Add($"Line {i + 1}: {ex.Message}");
			}
		}
		
		if (errors.Count > 0)
			return new DefinitionLoadResult { Success = false, Errors = errors };

		return new DefinitionLoadResult
		{
			Success = true,
			Definitions = defs
		};
	}
}