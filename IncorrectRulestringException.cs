
public class IncorrectRulestringException : System.Exception
{
    public IncorrectRulestringException() { }
    public IncorrectRulestringException(string message) : base(message) { }
    public IncorrectRulestringException(string message, System.Exception inner) : base(message, inner) { }
}