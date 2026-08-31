namespace Drive.Definitions;

public abstract record Definition;

public sealed record PartnerDefinition(string Name) : Definition;

public sealed record CompanyDefinition(string Name) : Definition;

public sealed record EmployeeDefinition(string Name, string CompanyName) : Definition;

public sealed record ContactDefinition(string EmployeeName, string PartnerName, string ContactType) : Definition;
