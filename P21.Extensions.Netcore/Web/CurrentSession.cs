using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace P21.Extensions.Web;

public static class CurrentSession // Changed from internal to public
{
    private const string SESSION_SINGLETON_NAME = "P21WebRules.SessionManager";
    private const string RULE_SESSION_KEY = "P21WebRules.SessionKey";

    public static IHttpContextAccessor? HttpContextAccessor { get; set; }

    public static WebBusinessRule GetCurrentRule()
    {
        if (HttpContextAccessor?.HttpContext?.Session == null)
        {
            throw new InvalidOperationException("Session or HttpContextAccessor is not available.");
        }

        var session = HttpContextAccessor.HttpContext.Session;

        if (!session.TryGetValue(RULE_SESSION_KEY, out byte[] ruleBytes))
        {
            var rule = new WebBusinessRule(
                HttpContextAccessor.HttpContext.RequestServices.GetService<IConfiguration>(),
                HttpContextAccessor
            );
            session.Set(RULE_SESSION_KEY, System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(rule));
            return rule;
        }

        return System.Text.Json.JsonSerializer.Deserialize<WebBusinessRule>(ruleBytes);
    }

    public static SessionSingleton GetSingleton()
    {
        if (HttpContextAccessor?.HttpContext?.Session == null)
        {
            throw new InvalidOperationException("Session or HttpContextAccessor is not available.");
        }

        var session = HttpContextAccessor.HttpContext.Session;

        if (!session.TryGetValue(SESSION_SINGLETON_NAME, out byte[] singletonBytes))
        {
            var singleton = new SessionSingleton();
            session.Set(SESSION_SINGLETON_NAME, System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(singleton));
            return singleton;
        }

        return System.Text.Json.JsonSerializer.Deserialize<SessionSingleton>(singletonBytes);
    }
}