namespace Drive;

public record Partner(string Name);

public record Company(string Name);

public record Employee(string Name, Company Company);

public enum ContactType
{
	Email,
	Call,
	Coffee
}

public record Contact(Employee Employee, Partner Partner, ContactType Type);
