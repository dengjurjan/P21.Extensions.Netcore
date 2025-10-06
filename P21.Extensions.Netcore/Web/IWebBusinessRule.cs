using Microsoft.Data.SqlClient;
using P21.Extensions.BusinessRule;
using System.Data;

namespace P21.Extensions.Web;

public interface IWebBusinessRule
{
    RuleResultData RuleResult { get; set; }

    RuleResult Execute();
    List<object> GetDatatableAsList(DataTable table);
    List<object> GetDatatableAsList(int tblIndex);
    List<object> GetDatatableAsList(string tableName);
    void Init(string brXML);
    bool IsInitialized();
    SqlConnection P21SqlConnection { get; }
    DataCollection Data { get; set; }
    void CleanupConnection();
    Session Session { get; }
}
