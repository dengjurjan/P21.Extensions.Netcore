using Microsoft.Data.SqlClient;
using System.Data;

namespace P21.Extensions.DataAccess;

internal class ConnectionManager
{
    private const string AppRoleInfoCommand = "SELECT p21_view_get_approle_info.value, p21_view_get_approle_info.name  FROM p21_view_get_approle_info; ";
    private const string AppRolePasswordValue = "p21_application_role";
    private const string AppRolePasswordDefaultValue = "p21_application_role_default";
    private const string AppRoleRoleName = "p21_application_role";
    private const string RestrictedAppRoleRoleName = "p21_application_role_business_rule";
    private const string RestrictedAppRolePasswordValue = "p21_application_role_business_rule";
    private const string AppRolePasswordEditedValue = "p21_application_role_password_edited";
    private const string AppRolePasswordEditedValueTrue = "Y";
    private const string AppRoleDefaultUser = "admin";

    public static P21Connection GetP21Connection(DBCredentials credentials, string rule)
    {
        var sqlConnection = new SqlConnection(GetP21ConnectionString(credentials, rule));
        sqlConnection.Open();
        return ApplyApplicationRole(sqlConnection, credentials);
    }

    public static void CloseP21Connection(P21Connection p21Connection)
    {
        _ = DisableApplicationRole(p21Connection);
        p21Connection.Connection.Close();
    }

    private static string GetP21ConnectionString(DBCredentials credentials, string rule)
    {
        string connectionString;
        if (!string.IsNullOrWhiteSpace(credentials.ConnectionString))
        {
            connectionString = credentials.ConnectionString;
        }
        else
        {
            var str1 = !credentials.RuleInAppName || string.IsNullOrEmpty(rule) ? "DynachangeRules" : "DynachangeRules - " + rule;
            if (string.IsNullOrEmpty(credentials.UserPassword))
            {
                connectionString = string.Format($"server={credentials.Server};Database={credentials.Database};User Id={credentials.UserID};Trusted_Connection=True;Application Name={str1};");
            }
            else
            {
                var str2 = P21Encryption.Decrypt(credentials.UserPassword, credentials.UserID);
                connectionString = string.Format($"server={credentials.Server};Database={credentials.Database};User Id={credentials.UserID};Password={str2};Trusted_Connection=False;Application Name={str1};");
            }
        }
        return connectionString;
    }

    private static P21Connection ApplyApplicationRole(
      SqlConnection sqlConnection,
      DBCredentials credentials)
    {
        if (sqlConnection.State != ConnectionState.Open)
        {
            throw new InvalidOperationException("Cannot apply the application role in a connection that is not already open. Use sqlConnection.Open() before applying the application role");
        }

        var cookie = (object)null;

        try
        {
            SqlCommand applicationRoleCommand = GetApplicationRoleCommand(sqlConnection, credentials);
            _ = applicationRoleCommand.ExecuteNonQuery();
            cookie = applicationRoleCommand.Parameters["@cookie"].SqlValue;
        }
        catch (SqlException)
        {
        }
        return new P21Connection(sqlConnection, cookie);
    }

    private static bool DisableApplicationRole(P21Connection p21Connection)
    {
        if (p21Connection.Cookie == null || p21Connection.Connection.State != ConnectionState.Open)
        {
            return false;
        }

        var sqlCommand = new SqlCommand("sp_unsetapprole", p21Connection.Connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        _ = sqlCommand.Parameters.AddWithValue("@cookie", p21Connection.Cookie);
        return sqlCommand.ExecuteNonQuery() == -1;
    }

    private static SqlCommand GetApplicationRoleCommand(
      SqlConnection sqlConnection,
      DBCredentials credentials)
    {
        var applicationRoleCommand = new SqlCommand("sp_setapprole", sqlConnection)
        {
            CommandType = CommandType.StoredProcedure
        };
        var str = credentials.cloudRestricted ? "p21_application_role_business_rule" : "p21_application_role";
        var appRolePassword = GetAppRolePassword(credentials);
        _ = applicationRoleCommand.Parameters.AddWithValue("@rolename", str);
        _ = applicationRoleCommand.Parameters.AddWithValue("@password", appRolePassword);
        _ = applicationRoleCommand.Parameters.AddWithValue("@fcreatecookie", true);
        applicationRoleCommand.Parameters.Add("@cookie", SqlDbType.VarBinary, 50).Direction = ParameterDirection.Output;
        return applicationRoleCommand;
    }

    private static string GetAppRolePassword(DBCredentials credentials)
    {
        var flag = false;
        var encryptedPW = "";
        var str1 = "";
        using (var connection = new SqlConnection(GetP21ConnectionString(credentials, string.Empty)))
        {
            connection.Open();
            SqlDataReader sqlDataReader = new SqlCommand("SELECT p21_view_get_approle_info.value, p21_view_get_approle_info.name  FROM p21_view_get_approle_info; ", connection).ExecuteReader();
            var str2 = credentials.cloudRestricted ? "p21_application_role_business_rule" : "p21_application_role";
            while (sqlDataReader.Read())
            {
                if (sqlDataReader.GetString(1) == str2)
                {
                    encryptedPW = sqlDataReader.GetString(0);
                }

                if (sqlDataReader.GetString(1) == "p21_application_role_default")
                {
                    str1 = sqlDataReader.GetString(0);
                }

                if (sqlDataReader.GetString(1) == "p21_application_role_password_edited")
                {
                    flag = sqlDataReader.GetString(0).Equals("Y", StringComparison.CurrentCultureIgnoreCase);
                }
            }
            if (!sqlDataReader.IsClosed)
            {
                sqlDataReader.Close();
            }

            connection.Close();
        }
        if (!flag && !credentials.cloudRestricted)
        {
            encryptedPW = str1;
        }

        return P21Encryption.Decrypt(encryptedPW, "admin");
    }

    internal static string Decrypt(string originalvalue, string key)
    {
        return P21Encryption.Decrypt(originalvalue, key);
    }

    internal static string Encrypt(string originalvalue, string key)
    {
        return P21Encryption.Encrypt(originalvalue, key);
    }
}
