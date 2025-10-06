namespace P21.Extensions.DataAccess;
using Microsoft.Data.SqlClient;
public class P21Connection
{
    public P21Connection(SqlConnection sqlConnection, object cookie)
    {
        Connection = sqlConnection;
        Cookie = cookie;
    }

    public SqlConnection Connection { get; private set; }

    public object Cookie { get; private set; }
}
