namespace P21.Extensions.Web;

public sealed class SessionSingleton
{
    internal SessionSingleton()
    {
    }

    public static SessionSingleton Current => CurrentSession.GetSingleton();

    public WebBusinessRule WebRule { get; set; }
}
