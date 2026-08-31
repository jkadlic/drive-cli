using FluentAssertions;

namespace Drive.Definitions;

[TestClass]
public class DefinitionParserTests
{
	private DefinitionParser Parser { get; set; } = null!;

	[TestInitialize]
	public void Init()
	{
		Parser = new DefinitionParser();
	}

	[TestMethod]
	[DataRow("Partner Erlich")]
	[DataRow("Partner Erlich ")]
	[DataRow("Partner Erlich  ")]
	[DataRow(" Partner Erlich ")]
	[DataRow("Partner  Erlich")]
	public void Parse_ValidPartner_ReturnsPartnerDefinition(string row)
	{
		// Act
		var def = Parser.Parse(row);

		// Assert
		def.Should().BeOfType<PartnerDefinition>().Which.Name.Should().Be("Erlich");
	}

	[TestMethod]
	[DataRow("Prtner Dinesh")]
	[DataRow("Engineer Gilfoyle")]
	public void Parse_InvalidDefinitionType_ThrowsParseException(string row)
	{
		// Act
		var act = () => Parser.Parse(row);

		// Assert
		act.Should().Throw<ParseException>();
	}

	[TestMethod]
	[DataRow("Company ACME")]
	[DataRow("Company  ACME")]
	public void Parse_ValidCompany_ReturnsCompanyDefinition(string row)
	{
		// Act
		var def = Parser.Parse(row);

		// Assert
		def.Should().BeOfType<CompanyDefinition>().Which.Name.Should().Be("ACME");
	}

	[TestMethod]
	[DataRow("Employee Bob ACME")]
	[DataRow("Employee  Bob   ACME")]
	[DataRow(" Employee Bob ACME ")]
	public void Parse_ValidEmployee_ReturnsEmployeeDefinition(string row)
	{
		// Act
		var def = Parser.Parse(row);

		// Assert
		var employeeDef = def.Should().BeOfType<EmployeeDefinition>().Which;
		employeeDef.Name.Should().Be("Bob");
		employeeDef.CompanyName.Should().Be("ACME");
	}

	[TestMethod]
	[DataRow("Contact Bob Chris email")]
	[DataRow("Contact  Bob  Chris  email")]
	public void Parse_ValidContact_ReturnsContactDefinition(string row)
	{
		// Act
		var def = Parser.Parse(row);

		// Assert
		var contactDef = def.Should().BeOfType<ContactDefinition>().Which;
		contactDef.EmployeeName.Should().Be("Bob");
		contactDef.PartnerName.Should().Be("Chris");
		contactDef.ContactType.Should().Be("email");
	}

	[TestMethod]
	[DataRow("partner Erlich")]
	[DataRow("PARTNER Erlich")]
	[DataRow("PaRtNeR Erlich")]
	public void Parse_TypeKeywordIsCaseInsensitive_ReturnsPartnerDefinition(string row)
	{
		// Act
		var def = Parser.Parse(row);

		// Assert
		def.Should().BeOfType<PartnerDefinition>().Which.Name.Should().Be("Erlich");
	}

	[TestMethod]
	[DataRow("Partner")]
	[DataRow("Partner Erlich Bachman")]
	public void Parse_PartnerWithWrongArgCount_ThrowsParseException(string row)
	{
		// Act
		var act = () => Parser.Parse(row);

		// Assert
		act.Should().Throw<ParseException>();
	}

	[TestMethod]
	[DataRow("Company")]
	[DataRow("Company ACME Corp")]
	public void Parse_CompanyWithWrongArgCount_ThrowsParseException(string row)
	{
		// Act
		var act = () => Parser.Parse(row);

		// Assert
		act.Should().Throw<ParseException>();
	}

	[TestMethod]
	[DataRow("Employee Bob")]
	[DataRow("Employee Bob ACME Extra")]
	public void Parse_EmployeeWithWrongArgCount_ThrowsParseException(string row)
	{
		// Act
		var act = () => Parser.Parse(row);

		// Assert
		act.Should().Throw<ParseException>();
	}

	[TestMethod]
	[DataRow("Contact Bob Chris")]
	[DataRow("Contact Bob Chris email Extra")]
	public void Parse_ContactWithWrongArgCount_ThrowsParseException(string row)
	{
		// Act
		var act = () => Parser.Parse(row);

		// Assert
		act.Should().Throw<ParseException>();
	}

	[TestMethod]
	[DataRow("")]
	[DataRow("   ")]
	public void Parse_EmptyOrWhitespaceRow_ThrowsParseException(string row)
	{
		// Act
		var act = () => Parser.Parse(row);

		// Assert
		act.Should().Throw<ParseException>();
	}

	[TestMethod]
	public void Parse_NullRow_ThrowsArgumentNullException()
	{
		// Act
		var act = () => Parser.Parse(null!);

		// Assert
		act.Should().Throw<ArgumentNullException>();
	}
}
