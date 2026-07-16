namespace Demo.Utils;

/// <summary>Minimal file + console logger.</summary>
public class Logger
{
    private readonly string _logDir;

    public Logger(string logDir = "output/logs")
    {
        _logDir = logDir;
        Directory.CreateDirectory(_logDir);
    }

    public void Info(string msg) => Write("INFO", msg);
    public void Warning(string msg) => Write("WARN", msg);
    public void Error(string msg) => Write("ERROR", msg);

    private void Write(string level, string msg)
    {
        var line = $"{DateTime.Now:HH:mm:ss} [{level}] {msg}";
        Console.WriteLine(line);
        var file = Path.Combine(_logDir, $"test_log_{DateTime.Now:yyyyMMdd}.log");
        File.AppendAllText(file, line + Environment.NewLine);
    }
}
