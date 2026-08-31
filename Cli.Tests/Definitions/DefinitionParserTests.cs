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
	public void Parse_ValidPartner_ReturnsDefinition(string row)
	{
		// Act
		var def = Parser.Parse(row);

		// Assert
		def.Should().NotBeNull();
		def.Type.Should().Be(DefinitionType.Partner);
		def.Parts.Should().HaveCount(1);
		def.Parts.ElementAt(0).Should().Be("Erlich");
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
	public void Parse_ValidCompany_ReturnsDefinition(string row)
	{
		// Act
		var def = Parser.Parse(row);

		// Assert
		def.Type.Should().Be(DefinitionType.Company);
		def.Parts.Should().Equal("ACME");
	}

	[TestMethod]
	[DataRow("Employee Bob ACME")]
	[DataRow("Employee  Bob   ACME")]
	[DataRow(" Employee Bob ACME ")]
	public void Parse_ValidEmployee_ReturnsDefinition(string row)
	{
		// Act
		var def = Parser.Parse(row);

		// Assert
		def.Type.Should().Be(DefinitionType.Employee);
		def.Parts.Should().Equal("Bob", "ACME");
	}

	[TestMethod]
	[DataRow("Contact Bob Chris email")]
	[DataRow("Contact  Bob  Chris  email")]
	public void Parse_ValidContact_ReturnsDefinition(string row)
	{
		// Act
		var def = Parser.Parse(row);

		// Assert
		def.Type.Should().Be(DefinitionType.Contact);
		def.Parts.Should().Equal("Bob", "Chris", "email");
	}

	[TestMethod]
	[DataRow("partner Erlich")]
	[DataRow("PARTNER Erlich")]
	[DataRow("PaRtNeR Erlich")]
	public void Parse_TypeKeywordIsCaseInsensitive_ReturnsDefinition(string row)
	{
		// Act
		var def = Parser.Parse(row);

		// Assert
		def.Type.Should().Be(DefinitionType.Partner);
		def.Parts.Should().Equal("Erlich");
	}

	[TestMethod]
	public void Parse_TypeKeywordWithNoName_ReturnsDefinitionWithEmptyParts()
	{
		// Act
		var def = Parser.Parse("Partner");

		// Assert
		def.Type.Should().Be(DefinitionType.Partner);
		def.Parts.Should().BeEmpty();
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