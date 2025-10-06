using System.Reflection;
using System.Text;

namespace P21.Extensions.BusinessRule;

public class LogString
{
    private readonly object _logLock = new();
    private string strLog = string.Empty;
    private readonly string path;

    private string FileName => $"{path}\\logs\\{Name}.log";

    public bool LineTerminate { get; set; } = true;

    public bool ReverseOrder { get; set; } = true;

    public int MaxFileMb { get; set; } = 1;

    public string Name { get; }

    private void ReadLog()
    {
        if (!File.Exists(FileName))
        {
            return;
        }

        lock (_logLock)
        {
            using (StreamReader streamReader = File.OpenText(FileName))
            {
                strLog = streamReader.ReadToEnd();
                streamReader.Close();
            }
        }
    }

    private void WriteLog()
    {
        lock (_logLock)
        {
            if (!Directory.Exists(path + "\\logs"))
            {
                _ = Directory.CreateDirectory(path + "\\logs");
            }

            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }

            if (strLog.Length == 0)
            {
                return;
            }

            using (StreamWriter text = File.CreateText(FileName))
            {
                text.Write(strLog);
                text.Close();
            }
        }
    }

    private void AddToLog(string stringToLog)
    {
        var str = $"{DateTime.Now.ToString()}: " + stringToLog;
        if (LineTerminate)
        {
            str += "\r\n";
        }

        lock (_logLock)
        {
            strLog = !ReverseOrder ? strLog + str : str + strLog;
            var length = MaxFileMb * 1048576 /*0x100000*/;
            if (Encoding.UTF8.GetByteCount(strLog) <= length)
            {
                return;
            }

            if (ReverseOrder)
            {
                strLog = strLog[..length];
            }
            else
            {
                strLog = strLog[^length..];
            }
        }
    }

    internal LogString(string logName, string logPath)
    {
        this.Name = logName;
        path = !string.IsNullOrWhiteSpace(logPath) ? logPath : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        ReadLog();
    }

    public LogString(string logName)
      : this(logName, null)
    {
    }

    public void Add(string stringToLog) => AddToLog(stringToLog);

    public void AddAndPersist(string stringToLog)
    {
        AddToLog(stringToLog);
        WriteLog();
    }

    public void Persist() => WriteLog();

    public void Clear()
    {
        lock (_logLock)
        {
            strLog = string.Empty;
        }

        WriteLog();
    }
}
