namespace P21.Extensions.BusinessRule;

[Serializable]
public class ResponseAttributes
{
    public ResponseAttributes(string responseTitle, string responseText, string callbackRule)
    {
        ResponseTitle = responseTitle;
        ResponseText = responseText;
        CallbackRule = callbackRule;
    }

    public ResponseAttributes()
    {
    }

    public string ResponseTitle { get; set; }

    public string ResponseText { get; set; }

    public string CallbackRule { get; set; }

    public ResponseField[] Fields { get; set; }

    public ResponseButton[] Buttons { get; set; }

    public string CallbackDataTableName { get; set; }

    public bool HideCloseButton { get; set; }
}
