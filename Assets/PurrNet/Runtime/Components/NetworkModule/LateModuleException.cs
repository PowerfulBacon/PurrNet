
using System;
using System.Runtime.Serialization;

public class LateModuleException : Exception
{
    public LateModuleException()
    {
    }

    public LateModuleException(string message) : base(message)
    {
    }

    public LateModuleException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected LateModuleException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
