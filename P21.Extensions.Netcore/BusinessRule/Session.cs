using System.Xml.Serialization;

namespace P21.Extensions.BusinessRule;

[XmlType(Namespace = "http://www.epicor.com/")]
[Serializable]
public class Session
{
    public Session()
    {
    }

    [Obsolete("Use parameterless constructor")]
    public Session(
      string userID,
      string version,
      string server,
      string database,
      string language,
      bool multiRow,
      string sessionId,
      string configurationID,
      string rfLocationID,
      string rfBinID,
      string applicationDisplayMode,
      string clientPlatform,
      string workstationID)
    {
        UserID = userID;
        Version = version;
        Server = server;
        Database = database;
        Language = language;
        MultiRow = multiRow;
        ID = sessionId;
        ConfigurationID = configurationID;
        RFLocationID = rfLocationID;
        RFBinID = rfBinID;
        ApplicationDisplayMode = applicationDisplayMode;
        ClientPlatform = clientPlatform;
        WorkstationID = workstationID;
    }

    public string UserID { get; set; }

    public string Version { get; set; }

    public string Server { get; set; }

    public string Database { get; set; }

    public string Language { get; set; }

    [Obsolete("Session.MultiRow is obsolete, please use RuleState.MultiRow instead.")]
    public bool MultiRow { get; set; }

    public string ID { get; set; }

    public string ConfigurationID { get; set; }

    public string RFLocationID { get; set; }

    public string RFBinID { get; set; }

    public string ApplicationDisplayMode { get; set; }

    public string ClientPlatform { get; set; }

    public string WorkstationID { get; set; }

    public string MiddlewareUrl { get; set; }

    public string RuleCodeLogPath { get; set; }
}
