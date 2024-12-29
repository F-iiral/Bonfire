namespace BonfireServer.Internal;

public enum LogLevel
{
    Info,
    Debug,
    Warning,
    Error,
}

public static class Logger
{
    private static LogLevel LogPriority = LogLevel.Info;

    public static void ChangeLogLevel(LogLevel logLevel)
    {
        LogPriority = logLevel;
    }

    private static void LogLine(string message)
    {
        message = $"[{DateTime.Now:HH:mm:ss}] - {message}";
        Console.WriteLine(message);
    }
    
    public static void Info(string message)
    {
        if (LogPriority > LogLevel.Info)
            return;
        
        Console.ResetColor();
        LogLine(message);
    }

    public static void Debug(string message)
    {
        if (LogPriority > LogLevel.Debug)
            return;

        Console.ForegroundColor = ConsoleColor.DarkMagenta;
        LogLine(message);
    }
    
    public static void Warn(string message)
    {
        if (LogPriority > LogLevel.Warning)
            return;
        
        Console.ForegroundColor = ConsoleColor.Yellow;
        LogLine(message);
    }
    
    public static void Error(string message)
    {
        if (LogPriority > LogLevel.Error)
            return;
        
        Console.ForegroundColor = ConsoleColor.Red;
        LogLine(message);
    }
}