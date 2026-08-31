# Drive Capital Network Analyzer

A command-line tool that ingests a list of declarations (Partner, Company, Employee, Contact), parses them into a
directed graph, and analyzes the relationship strength between the firm and each Company.

## Build, run, and test

Requires the [.NET SDK](https://dotnet.microsoft.com/) (targets `net10.0`).

**Build**

```bash
dotnet build
```

**Run**

Input is provided via a `--file` argument pointing at a plain-text file of definitions
(see `test.txt` for an example):

```bash
dotnet run --project Cli -- --file test.txt
```

> Note: Only file-based input is supported.

**Test**

```bash
dotnet test
```

## Design

This tool splits the problem into three core stages and organizes logic into a pipeline.

1. **Load text and parse into definitions** - Raw text is loaded into a pre-defined type and basic structure is validated.
2. **Parse definitions into a directed graph** - Definitions are loaded into an in-memory graph with wired references.
3. **Analyze relationship strength** - The analyzer walks the graph and, for each Company, finds the Partner with the strongest relationship (most Contact relationships).

Splitting the problem into these different stages supports future development of additional loading or analysis strategies.

**Parse, don't validate** - This tool evaluates errors in content and structure at parse time and returns a rich detail object at core steps. This allows parsing to share both responsibilities and provide accurate validation results.

**Tie-breaking** - The spec doesn't clarify what to do when Partners have an equal relationship strength. To keep output deterministic, this tool breaks ties based on alphabetical order.

**Input Ordering** - The tool supports any ordering of input definitions. This was intentionally included to support future flexibility with loading definitions from multiple sources at once, as well as to reduce assumptions required by the user.

## Use of LLM tools

This project used Claude Code in several ways:

1. Code Review - After major implementations, Claude Code was asked to review each piece for quality, bugs, code gaps, and adherence to the project spec.
2. Code Refactor - After code review, in specific cases, Claude Code was asked to perform a refactor (e.g. replacing DefinitionType with Definition discriminated union type).
3. Test Design - Claude Code was used to produce a testing spec for the core behavior of the tool.
4. Test Development - After testing spec definition and initial design of unit test structure (which packages, what to test, what not to test), Claude Code was used to expand unit-test coverage.

## Assumptions

- Each line of input includes exactly one definition.
- Contact type (`email`/`call`/`coffee`) is case-insensitive.
- Relationship strength is defined as a raw count of contacts between a Partner and a Company. There is no consideration for the type of contact made.
- Duplicate declarations of the same Partner, Company, or Employee are not shown as errors. This follows the assumption of well-formed input.
- Blank and whitespace-only lines in the input file are skipped.
