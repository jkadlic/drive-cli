using FluentAssertions;

namespace Drive.Definitions;

[TestClass]
public class DefinitionLoaderTests
{
	private readonly List<string> _tempFiles = [];

	[TestCleanup]
	public void Cleanup()
	{
		foreach (var path in _tempFiles)
		{
			if (File.Exists(path))
				File.Delete(path);
		}
	}

	private string CreateTempFile(params string[] lines)
	{
		var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.txt");
		File.WriteAllLines(path, lines);
		_tempFiles.Add(path);
		return path;
	}

	[TestMethod]
	public void LoadFromFile_ValidDefinitions_ReturnsSuccessWithParsedDefinitions()
	{
		var path = CreateTempFile("Partner Erlich", "Company ACME", "Employee Bob ACME", "Contact Bob Erlich email");

		var result = DefinitionLoader.LoadFromFile(path);

		result.Success.Should().BeTrue();
		result.Definitions.Should().HaveCount(4);
		result.Definitions![0].Should().BeOfType<PartnerDefinition>();
		result.Definitions[1].Should().BeOfType<CompanyDefinition>();
		result.Definitions[2].Should().BeOfType<EmployeeDefinition>();
		result.Definitions[3].Should().BeOfType<ContactDefinition>();
	}

	[TestMethod]
	public void LoadFromFile_BlankAndWhitespaceLines_AreSkipped()
	{
		var path = CreateTempFile("Partner Erlich", "", "   ", "Company ACME");

		var result = DefinitionLoader.LoadFromFile(path);

		result.Success.Should().BeTrue();
		result.Definitions.Should().HaveCount(2);
	}

	[TestMethod]
	public void LoadFromFile_EmptyFile_ReturnsSuccessWithNoDefinitions()
	{
		var path = CreateTempFile();

		var result = DefinitionLoader.LoadFromFile(path);

		result.Success.Should().BeTrue();
		result.Definitions.Should().BeEmpty();
	}

	[TestMethod]
	public void LoadFromFile_InvalidLine_ReturnsErrorWithOneBasedLineNumber()
	{
		var path = CreateTempFile("Partner Erlich", "Prtner Dinesh");

		var result = DefinitionLoader.LoadFromFile(path);

		result.Success.Should().BeFalse();
		result.Errors.Should().ContainSingle();
		result.Errors![0].Should().StartWith("Line 2:");
	}

	[TestMethod]
	public void LoadFromFile_MultipleInvalidLines_ReturnsAllErrorsWithCorrectLineNumbers()
	{
		var path = CreateTempFile("Prtner Dinesh", "Company ACME", "Partner");

		var result = DefinitionLoader.LoadFromFile(path);

		result.Success.Should().BeFalse();
		result.Errors.Should().HaveCount(2);
		result.Errors.Should().Contain(e => e.StartsWith("Line 1:"));
		result.Errors.Should().Contain(e => e.StartsWith("Line 3:"));
	}

	[TestMethod]
	public void LoadFromFile_MixOfValidAndInvalidLines_DoesNotReturnParsedDefinitions()
	{
		var path = CreateTempFile("Partner Erlich", "Prtner Dinesh");

		var result = DefinitionLoader.LoadFromFile(path);

		result.Success.Should().BeFalse();
		result.Definitions.Should().BeNull();
	}

	[TestMethod]
	public void LoadFromFile_FileDoesNotExist_ThrowsFileNotFoundException()
	{
		var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.txt");

		var act = () => DefinitionLoader.LoadFromFile(path);

		act.Should().Throw<FileNotFoundException>();
	}
}
