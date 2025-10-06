namespace P21.Extensions.BusinessRule;

[Serializable]
public class ResponseButton
{
    public ResponseButton()
    {
    }

    public ResponseButton(string buttonName, string buttonText, string buttonValue)
    {
        ButtonName = buttonName;
        ButtonText = buttonText;
        ButtonValue = buttonValue;
        IsDefaultButton = false;
    }

    public ResponseButton(
      string buttonName,
      string buttonText,
      string buttonValue,
      bool isDefaultButton)
      : this(buttonName, buttonText, buttonValue)
    {
        IsDefaultButton = isDefaultButton;
    }

    public string ButtonName { get; set; }

    public string ButtonText { get; set; }

    public string ButtonValue { get; set; }

    public bool IsDefaultButton { get; set; }

    public bool SkipRequiredCheck { get; set; }
}
