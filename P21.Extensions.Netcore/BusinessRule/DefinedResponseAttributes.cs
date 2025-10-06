namespace P21.Extensions.BusinessRule;

[Serializable]
public class DefinedResponseAttributes : ResponseAttributes
{
    private string _definedResponseWindowType;

    public string RequestString { get; set; }

    public string ResponseString { get; set; }

    public string DefinedResponseWindowType
    {
        get => _definedResponseWindowType;
        set
        {
            switch (value)
            {
                case "EPFHOSTEDTOKEN":
                    _definedResponseWindowType = value;
                    break;
                case "EPFRECEIPTPRINT":
                    _definedResponseWindowType = value;
                    break;
                default:
                    throw new Exception("Invalid DataType for DefinedResponseWindowType. Supported values: DefinedResponseWindowTypes.EPFHostedTokenPage/DefinedResponseWindowTypes.EPFReceiptPrint.");
            }
        }
    }

    public DefinedResponseAttributes(string definedResponseWindowType)
    {
        DefinedResponseWindowType = definedResponseWindowType;
    }
}
