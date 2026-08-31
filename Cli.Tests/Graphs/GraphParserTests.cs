using Drive.Definitions;
using FluentAssertions;

namespace Drive.Graphs;

[TestClass]
public class GraphParserTests
{
	[TestMethod]
	public void Parse_ValidDefinitions_ReturnsSuccessWithPopulatedGraph()
	{
		var definitions = new List<Definition>
		{
			new PartnerDefinition("Erlich"),
			new CompanyDefinition("ACME"),
			new EmployeeDefinition("Bob", "ACME"),
			new ContactDefinition("Bob", "Erlich", "email")
		};

		var result = GraphParser.Parse(definitions);

		result.Success.Should().BeTrue();
		result.Parsed!.Partners.Should().ContainKey("Erlich");
		result.Parsed.Companies.Should().ContainKey("ACME");
		result.Parsed.Employees.Should().ContainKey("Bob");
		result.Parsed.Employees["Bob"].Company.Name.Should().Be("ACME");
		result.Parsed.Contacts.Should().ContainSingle();
		result.Parsed.Contacts[0].Employee.Name.Should().Be("Bob");
		result.Parsed.Contacts[0].Partner.Name.Should().Be("Erlich");
		result.Parsed.Contacts[0].Type.Should().Be(ContactType.Email);
	}

	[TestMethod]
	public void Parse_DefinitionsOutOfDependencyOrder_StillSucceeds()
	{
		var definitions = new List<Definition>
		{
			new ContactDefinition("Bob", "Erlich", "email"),
			new EmployeeDefinition("Bob", "ACME"),
			new PartnerDefinition("Erlich"),
			new CompanyDefinition("ACME")
		};

		var result = GraphParser.Parse(definitions);

		result.Success.Should().BeTrue();
		result.Parsed!.Contacts.Should().ContainSingle();
	}

	[TestMethod]
	public void Parse_EmployeeWithUnknownCompany_ReturnsError()
	{
		var definitions = new List<Definition> { new EmployeeDefinition("Bob", "ACME") };

		var result = GraphParser.Parse(definitions);

		result.Success.Should().BeFalse();
		result.Errors.Should().ContainSingle();
		result.Errors![0].Message.Should().Contain("ACME");
	}

	[TestMethod]
	public void Parse_ContactWithUnknownEmployee_ReturnsError()
	{
		var definitions = new List<Definition>
		{
			new PartnerDefinition("Erlich"),
			new ContactDefinition("Bob", "Erlich", "email")
		};

		var result = GraphParser.Parse(definitions);

		result.Success.Should().BeFalse();
		result.Errors.Should().ContainSingle();
		result.Errors![0].Message.Should().Contain("Bob");
	}

	[TestMethod]
	public void Parse_ContactWithUnknownPartner_ReturnsError()
	{
		var definitions = new List<Definition>
		{
			new CompanyDefinition("ACME"),
			new EmployeeDefinition("Bob", "ACME"),
			new ContactDefinition("Bob", "Erlich", "email")
		};

		var result = GraphParser.Parse(definitions);

		result.Success.Should().BeFalse();
		result.Errors.Should().ContainSingle();
		result.Errors![0].Message.Should().Contain("Erlich");
	}

	[TestMethod]
	public void Parse_ContactWithInvalidContactType_ReturnsError()
	{
		var definitions = new List<Definition>
		{
			new PartnerDefinition("Erlich"),
			new CompanyDefinition("ACME"),
			new EmployeeDefinition("Bob", "ACME"),
			new ContactDefinition("Bob", "Erlich", "carrierpigeon")
		};

		var result = GraphParser.Parse(definitions);

		result.Success.Should().BeFalse();
		result.Errors.Should().ContainSingle();
		result.Errors![0].Message.Should().Contain("Invalid ContactType");
	}

	[TestMethod]
	[DataRow("email", ContactType.Email)]
	[DataRow("EMAIL", ContactType.Email)]
	[DataRow("Call", ContactType.Call)]
	[DataRow("COFFEE", ContactType.Coffee)]
	public void Parse_ContactTypeIsCaseInsensitive_ReturnsExpectedType(string rawType, ContactType expected)
	{
		var definitions = new List<Definition>
		{
			new PartnerDefinition("Erlich"),
			new CompanyDefinition("ACME"),
			new EmployeeDefinition("Bob", "ACME"),
			new ContactDefinition("Bob", "Erlich", rawType)
		};

		var result = GraphParser.Parse(definitions);

		result.Success.Should().BeTrue();
		result.Parsed!.Contacts[0].Type.Should().Be(expected);
	}

	[TestMethod]
	public void Parse_ContactWithMultipleFailures_ReturnsAnErrorForEach()
	{
		var definitions = new List<Definition> { new ContactDefinition("Bob", "Erlich", "carrierpigeon") };

		var result = GraphParser.Parse(definitions);

		result.Success.Should().BeFalse();
		result.Errors.Should().HaveCount(3);
	}

	[TestMethod]
	public void Parse_MultipleContactsBetweenSameEmployeeAndPartner_AreAllRetained()
	{
		var definitions = new List<Definition>
		{
			new PartnerDefinition("Erlich"),
			new CompanyDefinition("ACME"),
			new EmployeeDefinition("Bob", "ACME"),
			new ContactDefinition("Bob", "Erlich", "email"),
			new ContactDefinition("Bob", "Erlich", "call")
		};

		var result = GraphParser.Parse(definitions);

		result.Success.Should().BeTrue();
		result.Parsed!.Contacts.Should().HaveCount(2);
	}

	[TestMethod]
	public void Parse_NoDefinitions_ReturnsSuccessWithEmptyGraph()
	{
		var result = GraphParser.Parse(new List<Definition>());

		result.Success.Should().BeTrue();
		result.Parsed!.Partners.Should().BeEmpty();
		result.Parsed.Companies.Should().BeEmpty();
		result.Parsed.Employees.Should().BeEmpty();
		result.Parsed.Contacts.Should().BeEmpty();
	}
}
