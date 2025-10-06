using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using P21.Extensions.BusinessRule;
using P21.Extensions.DataAccess;
using System.Data;
using System.Dynamic;

namespace P21.Extensions.Web;

public class WebBusinessRule : BusinessRule.Rule, IWebBusinessRule
{
    private readonly IConfiguration _configuration;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private bool initComplete;

    public WebBusinessRule(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _httpContextAccessor = httpContextAccessor;
    }

    private string GetConnectionString()
    {
        return _configuration.GetConnectionString("P21ConnectionString");
    }

    public void Init(string brXML)
    {
        if (string.IsNullOrWhiteSpace(brXML))
        {
            return;
        }

        var credentials = new DBCredentials();
        Initialize(brXML, credentials);
        var connectionString = GetConnectionString();
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            credentials.ConnectionString = connectionString;
        }
        else
        {
            // Fallback to session-based credentials
            var session = _httpContextAccessor.HttpContext?.Session;
            credentials.UserID = session?.GetString("P21UserID");
            credentials.Database = session?.GetString("P21Database");
            credentials.Server = session?.GetString("P21Server");
        }
        RuleResult = new RuleResultData();
        initComplete = true;
    }

    public bool IsInitialized() => initComplete;

    public override RuleResult Execute()
    {
        throw new NotImplementedException();
    }

    public List<object> GetDatatableAsList(DataTable table)
    {
        var datatableAsList = new List<object>();
        foreach (DataRow row in (InternalDataCollectionBase)table.Rows)
        {
            var dictionary = (IDictionary<string, object>)new ExpandoObject();
            foreach (DataColumn column in (InternalDataCollectionBase)table.Columns)
            {
                dictionary.Add(column.Caption, row[column.ColumnName]);
            }
            datatableAsList.Add(dictionary);
        }
        return datatableAsList;
    }

    public List<object> GetDatatableAsList(string tableName)
    {
        DataTable table = Data.Set.Tables[tableName];
        return table != null ? GetDatatableAsList(table) : null;
    }

    public List<object> GetDatatableAsList(int tblIndex)
    {
        DataTable table = Data.Set.Tables[tblIndex];
        return table != null ? GetDatatableAsList(table) : null;
    }

    public RuleResultData RuleResult { get; set; }
}