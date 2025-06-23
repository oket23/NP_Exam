using Serilog;
using Serilog.Core;

namespace LoggerSevice;

public static class LoggerService
{
    public static Logger GetLogger()
    {
        var logPath = "../../../../Logs/ProTeamsMircoServiceLogs.txt";

        var logDirectory = Path.GetDirectoryName(logPath);
        Directory.CreateDirectory(logDirectory);

        return new LoggerConfiguration()
               .WriteTo.File(logPath, outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [ProTeamsMircoService]: {Message:lj}{NewLine}{Exception}")
               .MinimumLevel.Debug()
               .CreateLogger();
    }

}



