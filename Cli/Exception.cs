namespace Drive;

public class ParseException : Exception
{
	public ParseException() : base() { }
	public ParseException(string message) : base(message) { }
}