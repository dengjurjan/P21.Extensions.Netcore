using Microsoft.AspNetCore.Mvc;
using P21.Extensions.BusinessRule;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace P21.Extensions.Web;

public class InitializeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public IWebBusinessRule WebBusinessRule { get; }

    public InitializeController(IHttpClientFactory httpClientFactory, IWebBusinessRule webBusinessRule)
    {
        _httpClientFactory = httpClientFactory;
        WebBusinessRule = webBusinessRule;
    }

    private IActionResult ToStatusCodeResult(HttpStatusCode statusCode, string message = null)
    {
        return StatusCode((int)statusCode, message);
    }

    [HttpPost]
    public IActionResult Index(string ruleController, string ruleAction)
    {
        try
        {
            var s = Request.Form["vbrData"].ToString();
            var str1 = Request.Form["token"].ToString();
            var str2 = Request.Form["soaURL"].ToString();
            if (string.IsNullOrWhiteSpace(s))
            {
                return ToStatusCodeResult(HttpStatusCode.BadRequest, "Some of the required data passed to Initialize is missing.");
            }

            WebBusinessRule.Init(Encoding.UTF8.GetString(Convert.FromBase64String(s)));
            var flag = false;
            if (WebBusinessRule.IsInitialized())
            {
                var applicationDisplayMode = WebBusinessRule.Session.ApplicationDisplayMode;
                flag = string.IsNullOrEmpty(WebBusinessRule.Session.ClientPlatform) &&
                       (applicationDisplayMode == "sdi" || applicationDisplayMode == "mdi");
            }
            if (!flag)
            {
                if (string.IsNullOrWhiteSpace(str2) || string.IsNullOrWhiteSpace(str1))
                {
                    return ToStatusCodeResult(HttpStatusCode.BadRequest, "Some of the required data passed to Initialize is missing.");
                }

                if (!str2.EndsWith("/"))
                {
                    str2 += "/";
                }

                var address = str2 + "api/users/ping";
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", str1);
                var response = client.GetStringAsync(address).GetAwaiter().GetResult();
            }
        }
        catch (Exception ex)
        {
            return ToStatusCodeResult(HttpStatusCode.BadRequest, "Token validation was rejected - " + ex.Message);
        }

        if (!WebBusinessRule.IsInitialized())
        {
            return ToStatusCodeResult(HttpStatusCode.InternalServerError, "Rule was not initialized properly.");
        }

        if (string.IsNullOrWhiteSpace(ruleController))
        {
            ruleController = "Home";
        }

        if (string.IsNullOrWhiteSpace(ruleAction))
        {
            ruleAction = nameof(Index);
        }

        return RedirectToAction(ruleAction, ruleController);
    }

    public IActionResult CloseImmediately()
    {
        RuleResultData ruleResult = WebBusinessRule.RuleResult;
        return ruleResult == null
            ? ToStatusCodeResult(HttpStatusCode.BadRequest)
            : Content(PrepareJson(ruleResult), "application/json");
    }

    public IActionResult Close()
    {
        RuleResultData ruleResult = WebBusinessRule.RuleResult;
        return ruleResult == null
            ? ToStatusCodeResult(HttpStatusCode.BadRequest)
            : Content($"<script>window.parent.postMessage('{PrepareJson(ruleResult)}', '*');</script>", "text/html");
    }

    public static string SanitizeResultMessage(string ruleMessage)
    {
        ruleMessage = ruleMessage.Replace("\"", "\\\"");
        return ruleMessage;
    }

    public string PrepareJson(RuleResultData ruleResult)
    {
        WebBusinessRule.CleanupConnection();
        if (WebBusinessRule.Data.UpdateByOrderCoded)
        {
            WebBusinessRule.Data.OrderByUpdateSequence();
        }

        ruleResult.Xml = WebBusinessRule.Data.ToXml();
        ruleResult.Xml = Convert.ToBase64String(Encoding.UTF8.GetBytes(ruleResult.Xml));
        ruleResult.Message = SanitizeResultMessage(ruleResult.Message);

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null, // Use exact property names (matches Newtonsoft's default)
            Converters = { new JsonStringEnumConverter() } // Serialize enums as strings
        };
        return JsonSerializer.Serialize(ruleResult, options);
    }
}