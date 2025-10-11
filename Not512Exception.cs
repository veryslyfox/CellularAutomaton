[System.Serializable]
public class Not256Exception : System.Exception
{
    public Not256Exception() { }
    public Not256Exception(string message) : base(message) { }
    public Not256Exception(string message, System.Exception inner) : base(message, inner) { }
    protected Not256Exception(
        System.Runtime.Serialization.SerializationInfo info,
        System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
}