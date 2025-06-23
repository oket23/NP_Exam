
using HeroMicroService.Services;
using LoggerSevice;

namespace HeroMicroService;

public class Program
{
    static async Task Main(string[] args)
    {
        var logger = LoggerService.GetLogger();
        var heroService = new HeroService(logger);
        var listenerService = new ListenerService(logger, heroService);

        await listenerService.ListenerHandlerAsync();
    }
}

