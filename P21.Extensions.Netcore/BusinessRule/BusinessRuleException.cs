using System.Runtime.Serialization;

namespace P21.Extensions.BusinessRule;

[Serializable]
public sealed class BusinessRuleException : Exception
{
    public BusinessRuleException()
    {
    }

    public BusinessRuleException(string message)
      : base(message)
    {
    }

    public BusinessRuleException(string message, Exception innerEx)
      : base(message, innerEx)
    {
    }

    private BusinessRuleException(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }
}
