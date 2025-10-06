using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Net;
using P21.Extensions.BusinessRule;

namespace P21.Extensions.Web;

public class BaseRuleController(IWebBusinessRule webBusinessRule) : Controller
{
    protected DataCollection Data => Rule.Data;

    protected SqlConnection P21SqlConnection => Rule.P21SqlConnection;

    protected IWebBusinessRule Rule => WebBusinessRule;//.Current;

    public IWebBusinessRule WebBusinessRule { get; } = webBusinessRule;

    private IActionResult ToStatusCodeResult(HttpStatusCode statusCode, string message = null)
    {
        return StatusCode((int)statusCode, message);
    }

    public IActionResult PerformPreRun()
    {
        RuleResultData ruleResult = WebBusinessRule.RuleResult;
        if (ruleResult == null)
        {
            return ToStatusCodeResult(HttpStatusCode.BadRequest);
        }

        ruleResult.ContinueAfterPreRun = PreRun();
        return RedirectToAction("CloseImmediately", "Initialize", new { area = "" });
    }

    protected virtual bool PreRun() => true;
}