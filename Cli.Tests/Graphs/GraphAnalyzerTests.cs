using FluentAssertions;

namespace Drive.Graphs;

[TestClass]
public class GraphAnalyzerTests
{
	private readonly CompanyRelationshipAnalyzer _analyzer = new();

	private static Graph BuildGraph(
		IEnumerable<Partner> partners,
		IEnumerable<Company> companies,
		IEnumerable<Employee> employees,
		IEnumerable<Contact> contacts) =>
		new()
		{
			Partners = partners.ToDictionary(p => p.Name),
			Companies = companies.ToDictionary(c => c.Name),
			Employees = employees.ToDictionary(e => e.Name),
			Contacts = contacts.ToList()
		};

	[TestMethod]
	public void Analyze_CompanyWithNoContacts_ReturnsNoRelationship()
	{
		var acme = new Company("ACME");
		var graph = BuildGraph([], [acme], [], []);

		var result = _analyzer.Analyze(graph);

		result.Entries.Should().ContainSingle();
		result.Entries[0].HasRelationship.Should().BeFalse();
		result.Entries[0].Company.Should().Be(acme);
	}

	[TestMethod]
	public void Analyze_CompanyWithSingleContact_ReturnsThatPartnerWithStrengthOne()
	{
		var erlich = new Partner("Erlich");
		var acme = new Company("ACME");
		var bob = new Employee("Bob", acme);
		var graph = BuildGraph([erlich], [acme], [bob], [new Contact(bob, erlich, ContactType.Email)]);

		var result = _analyzer.Analyze(graph);

		result.Entries.Should().ContainSingle();
		result.Entries[0].HasRelationship.Should().BeTrue();
		result.Entries[0].Partner.Should().Be(erlich);
		result.Entries[0].Strength.Should().Be(1);
	}

	[TestMethod]
	public void Analyze_CompanyWithMultiplePartners_ReturnsPartnerWithMostContacts()
	{
		var erlich = new Partner("Erlich");
		var jared = new Partner("Jared");
		var acme = new Company("ACME");
		var bob = new Employee("Bob", acme);
		var contacts = new List<Contact>
		{
			new(bob, erlich, ContactType.Email),
			new(bob, erlich, ContactType.Call),
			new(bob, erlich, ContactType.Coffee),
			new(bob, jared, ContactType.Email)
		};
		var graph = BuildGraph([erlich, jared], [acme], [bob], contacts);

		var result = _analyzer.Analyze(graph);

		result.Entries[0].Partner.Should().Be(erlich);
		result.Entries[0].Strength.Should().Be(3);
	}

	[TestMethod]
	public void Analyze_ContactsCountRegardlessOfContactType()
	{
		var erlich = new Partner("Erlich");
		var acme = new Company("ACME");
		var bob = new Employee("Bob", acme);
		var contacts = new List<Contact>
		{
			new(bob, erlich, ContactType.Email),
			new(bob, erlich, ContactType.Call),
			new(bob, erlich, ContactType.Coffee)
		};
		var graph = BuildGraph([erlich], [acme], [bob], contacts);

		var result = _analyzer.Analyze(graph);

		result.Entries[0].Strength.Should().Be(3);
	}

	[TestMethod]
	public void Analyze_MultipleEmployeesAtSameCompany_ContactsAggregatedByPartner()
	{
		var erlich = new Partner("Erlich");
		var acme = new Company("ACME");
		var bob = new Employee("Bob", acme);
		var dinesh = new Employee("Dinesh", acme);
		var contacts = new List<Contact>
		{
			new(bob, erlich, ContactType.Email),
			new(dinesh, erlich, ContactType.Call)
		};
		var graph = BuildGraph([erlich], [acme], [bob, dinesh], contacts);

		var result = _analyzer.Analyze(graph);

		result.Entries[0].Partner.Should().Be(erlich);
		result.Entries[0].Strength.Should().Be(2);
	}

	[TestMethod]
	public void Analyze_ContactsWithDifferentCompanies_AreNotConflated()
	{
		var erlich = new Partner("Erlich");
		var acme = new Company("ACME");
		var globex = new Company("Globex");
		var bob = new Employee("Bob", acme);
		var dinesh = new Employee("Dinesh", globex);
		var contacts = new List<Contact>
		{
			new(bob, erlich, ContactType.Email),
			new(dinesh, erlich, ContactType.Email),
			new(dinesh, erlich, ContactType.Call)
		};
		var graph = BuildGraph([erlich], [acme, globex], [bob, dinesh], contacts);

		var result = _analyzer.Analyze(graph);

		var acmeEntry = result.Entries.Single(e => e.Company.Name == "ACME");
		var globexEntry = result.Entries.Single(e => e.Company.Name == "Globex");
		acmeEntry.Strength.Should().Be(1);
		globexEntry.Strength.Should().Be(2);
	}

	[TestMethod]
	public void Analyze_ResultsAreOrderedAlphabeticallyByCompanyName()
	{
		var zorp = new Company("Zorp");
		var acme = new Company("ACME");
		var globex = new Company("Globex");
		var graph = BuildGraph([], [zorp, acme, globex], [], []);

		var result = _analyzer.Analyze(graph);

		result.Entries.Select(e => e.Company.Name).Should().Equal("ACME", "Globex", "Zorp");
	}

	[TestMethod]
	public void Analyze_TieBetweenPartners_ReturnsAlphabeticallyFirstPartnerName()
	{
		var erlich = new Partner("Erlich");
		var jared = new Partner("Jared");
		var acme = new Company("ACME");
		var bob = new Employee("Bob", acme);
		var contacts = new List<Contact>
		{
			new(bob, jared, ContactType.Email),
			new(bob, erlich, ContactType.Email)
		};
		var graph = BuildGraph([erlich, jared], [acme], [bob], contacts);

		var result = _analyzer.Analyze(graph);

		result.Entries[0].Partner.Should().Be(erlich);
		result.Entries[0].Strength.Should().Be(1);
	}
}
