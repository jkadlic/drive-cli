using System.Diagnostics.CodeAnalysis;

namespace Drive.Graphs;

/// <summary>
/// Defines a relationship between the firm and an external company. If a relationship exists, this includes the partner
/// with the strongest relationship, and the strength of that relationship.
/// </summary>
public class CompanyRelationshipEntry
{
	[MemberNotNullWhen(true, nameof(Partner))]
	[MemberNotNullWhen(true, nameof(Strength))]
	public bool HasRelationship { get; init; }
	public required Company Company { get; init; }
	public Partner? Partner { get; init; }
	public int? Strength { get; init; }

	public override string ToString() => HasRelationship 
		? $"{Company.Name}: {Partner.Name} ({Strength})"
		: $"{Company.Name}: No current relationship";
}

/// <summary>
/// Analysis result container.
/// </summary>
public class CompanyRelationshipAnalysis
{
	public required List<CompanyRelationshipEntry> Entries { get; init; }

	public override string ToString() => string.Join("\n", Entries);
}

/// <summary>
/// Analyzes the relationship strength between the firm and external companies via <see cref="Partner"/>'s and <see cref="Employee"/>'s.
/// </summary>
public class CompanyRelationshipAnalyzer : IAnalyzer<CompanyRelationshipAnalysis>
{
	public CompanyRelationshipAnalysis Analyze(Graph graph)
	{
		var entries = new List<CompanyRelationshipEntry>();
		
		var contactGroups = graph.Contacts
			.GroupBy(x => (x.Employee.Company.Name, x.Partner.Name))
			.ToList();

		foreach (var c in graph.Companies.OrderBy(x => x.Value.Name))
		{
			var m = contactGroups
				.Where(x => x.Key.Item1 == c.Key)
				.MaxBy(x => x.Count());

			if (m != null)
			{
				var company = graph.Companies[m.Key.Item1];
				var partner = graph.Partners[m.Key.Item2];
				
				entries.Add(new CompanyRelationshipEntry
				{
					HasRelationship = true,
					Company = company,
					Partner = partner,
					Strength = m.Count()
				});
			}
			else
			{
				var company = graph.Companies[c.Key];
				entries.Add(new CompanyRelationshipEntry { HasRelationship = false, Company = company });
			}
		}
		
		return new CompanyRelationshipAnalysis { Entries = entries };
	}
}