namespace P21.Extensions.DataAccess;

[Serializable]
public class DBCredentials
{
    public DBCredentials(string userID, string userPassword, string server, string database)
    {
        UserID = userID;
        UserPassword = userPassword;
        Server = server;
        Database = database;
    }

    public DBCredentials()
    {
    }

    public string ConnectionString { get; set; }

    public string UserID { get; set; }

    public string UserPassword { get; set; }

    public string Server { get; set; }

    public string Database { get; set; }

    public bool cloudRestricted { get; set; }

    public bool RuleInAppName { get; set; }
}
