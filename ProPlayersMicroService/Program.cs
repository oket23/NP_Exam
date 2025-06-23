using LoggerSevice;
using ProPlayersMicroService.Service;

namespace ProPlayersMicroService;

public class Program
{
    static async Task Main(string[] args)
    {
        var logger = LoggerService.GetLogger();
        var proPlayersService = new ProPlayersService(logger);
        var listenerService = new ListenerService(logger, proPlayersService);

        await listenerService.ListenerHandlerAsync();
    }
}
