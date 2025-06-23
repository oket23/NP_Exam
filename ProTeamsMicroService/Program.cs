using LoggerSevice;
using ProTeamsMicroService.Services;

namespace ProTeamsMicroService;

public class Program
{
    static async Task Main(string[] args)
    {
        var logger = LoggerService.GetLogger();
        var proTeamsService = new ProTeamsService(logger);
        var listenerService = new ListenerService(logger, proTeamsService);

        await listenerService.ListenerHandlerAsync();
    }
}
