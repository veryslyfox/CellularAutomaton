class IncorrectRulestringException : Exception
{
    public IncorrectRulestringException(string message)
    {
        Message = message;
    }

    public override string Message { get; }
}