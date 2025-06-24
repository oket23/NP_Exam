using LoggerSevice;
using Serilog.Core;
using UI.Services;

namespace UI;

public class Program
{
    static async Task Main(string[] args)
    {
        var logger = LoggerService.GetLogger();
        var heroService = new HeroService(logger);
        var proPlayerService = new ProPlayersService(logger);
        var proTeamService = new ProTeamsService(logger);
        var telegramService = new TelegramBotService(logger, proPlayerService,proTeamService,heroService);
        telegramService.StartTelegramBot();

        while (true)
        {
            ShowMainMenu();
            var option = Console.ReadLine();

            switch (option)
            {
                default:
                    Console.WriteLine("\nEnter correct option!\n");
                    break;
                case "1":
                    await HeroMenuHandlerAsync(heroService, logger);
                    break;
                case "2":
                    await ProPlayerMenuHandler(proPlayerService,logger);
                    break;
                case "3":
                    await ProTeamsMenuHandler(proTeamService, logger);
                    break;
                case "4":
                    Console.WriteLine("\nBye-bye...");
                    return;
            }
        }

    }

    static void ShowMainMenu()
    {
        Console.Write("1.Hero menu\n2.Pro players menu\n3.Pro teams menu\n4.Exit\nChoose option: ");
    }

    static void ShowProTeamsMenu()
    {
        Console.Write("\n1.Get pro teams\n2.Get pro teams and favorite hero\n3.Exit\nChoose option: ");
    }

    static void ShowHeroMenu()
    {
        Console.Write("\n1.Get all hero\n2.Get hero benchmarks\n3.Get hero for id\n4.Add hero\n5.Update hero\n6.Delete hero\n7.Exit\nChoose option: ");
    }

    static void ShowProPlayersMenu()
    {
        Console.Write("\n1.Get pro players\n2.Exit\nChoose option: ");
    }

    static async Task HeroMenuHandlerAsync(HeroService heroService, Logger logger)
    {
        ShowHeroMenu();
        var option = Console.ReadLine();

        switch (option)
        {
            default:
                Console.WriteLine("\nEnter correct option!\n");
                break;
            case "1":
                try
                {
                    var heros = await heroService.GetAllHeroAsync();

                    Console.WriteLine("\nAll heroes:\n");
                    foreach (var hero in heros)
                    {
                        Console.WriteLine(hero);
                    }
                    Console.WriteLine();

                    logger.Information("UI show all heroes");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n{ex.Message}\n");
                    logger.Error($"{ex.Message}");
                }
                break;
            case "2":
                try
                {
                    Console.Write("\nEnter hero id: ");
                    if(!int.TryParse(Console.ReadLine(), out int heroId))
                    {
                        Console.WriteLine("\nEnter corrent id!\n");
                        logger.Information("User entered an incorrect hero id");
                        return;
                    }

                    var heroBenchmark = await heroService.GetHeroBenchmarcksAsync(heroId);
                    var hero = await heroService.GetHeroByIdAsync(heroId);

                    Console.WriteLine($"\n{hero.Name} (id: {hero.Id}) benchmarks:");
                    Console.WriteLine(heroBenchmark);
                    logger.Information($"UI show hero {heroId} benchmark");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n{ex.Message}\n");
                    logger.Error($"{ex.Message}");
                }
                break;
            case "3":
                try
                {
                    Console.Write("\nEnter hero id: ");
                    if (!int.TryParse(Console.ReadLine(), out int heroId))
                    {
                        Console.WriteLine("\nEnter corrent id!\n");
                        logger.Information("User entered an incorrect hero id");
                        return;
                    }

                    var hero = await heroService.GetHeroByIdAsync(heroId);

                    Console.WriteLine($"\n{hero}");
                    logger.Information($"UI show hero {heroId} benchmark");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n{ex.Message}\n");
                    logger.Error($"{ex.Message}");
                }
                break;
            case "4":
                try
                {
                    var POSTHero = heroService.GetValidHero(false);

                    var result = await heroService.PostHeroAsync(POSTHero);

                    Console.WriteLine($"\n{result}");
                    logger.Information($"UI addet hero by id: {POSTHero.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n{ex.Message}\n");
                    logger.Error($"{ex.Message}");
                }
                break;
            case "5":
                try
                {
                    var PUTHero = heroService.GetValidHero(true);

                    var result = await heroService.PutHeroAsync(PUTHero);

                    Console.WriteLine($"\n{result}");
                    logger.Information($"UI updated hero by id: {PUTHero.Id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n{ex.Message}\n");
                    logger.Error($"{ex.Message}");
                }
                break;
            case "6":
                try
                {
                    Console.Write("\nEnter hero id: ");
                    if(!int.TryParse(Console.ReadLine(), out int id))
                    {
                        Console.WriteLine("\nEnter corrent id!\n");
                        logger.Information("User entered an incorrect hero id");
                        return;
                    }

                    var result = await heroService.DeleteHeroAsync(id);

                    Console.WriteLine($"\n{result}");
                    logger.Information($"UI deleted hero by id: {id}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n{ex.Message}\n");
                    logger.Error($"{ex.Message}");
                }
                break;
            case "7":
                Console.WriteLine("\nExiting...\n");
                return;

        }
    }
    static async Task ProPlayerMenuHandler(ProPlayersService playersService, Logger logger)
    {
        ShowProPlayersMenu();
        var option = Console.ReadLine();

        switch (option)
        {
            default:
                Console.WriteLine("\nEnter correct option!\n");
                break;
            case "1":
                try
                {
                    var proPlayers = await playersService.GetProPlayersAsync(10,2);

                    Console.WriteLine("\nAll pro players:");
                    foreach (var player in proPlayers)
                    {
                        Console.WriteLine(player);
                    }
                    Console.WriteLine();

                    logger.Information($"UI show pro player");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n{ex.Message}\n");
                    logger.Error($"{ex.Message}");
                }
                break;
            case "2":
                Console.WriteLine("\nExiting...\n");
                break;
        }
    }
    static async Task ProTeamsMenuHandler(ProTeamsService teamsService, Logger logger)
    {
        ShowProTeamsMenu();
        var option = Console.ReadLine();

        switch (option)
        {
            default:
                Console.WriteLine("\nEnter correct option!\n");
                break;
            case "1":
                try
                {
                    var proTeams = await teamsService.GetProTeamsAsync(5,1);

                    Console.WriteLine("\nAll teams:");
                    foreach (var teams in proTeams)
                    {
                        Console.WriteLine(teams);
                    }

                    logger.Information($"UI show pro teams");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n{ex.Message}\n");
                    logger.Error($"{ex.Message}");
                }
                break;
            case "2":
                try
                {
                    var proTeamsF = await teamsService.GetProTeamsAndFavoriteHeroAsync(5,5);
                    Console.WriteLine("\nAll teams and favorite hero:");
                    foreach (var teams in proTeamsF)
                    {
                        Console.WriteLine(teams);
                    }
                    
                    logger.Information($"UI show pro teams");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n{ex.Message}\n");
                    logger.Error($"{ex.Message}");
                }
                break;
            case "3":
                Console.WriteLine("\nExiting....\n");
                break;
        }
    }

}
