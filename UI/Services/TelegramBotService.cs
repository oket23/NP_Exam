using Exam.Models;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using System.Text;
using System.Text.RegularExpressions;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using UI.Models.HeroService;

namespace UI.Services;

public class TelegramBotService
{
    private Logger _logger;
    private TelegramBotClient _client;
    private User _bot;
    private ProTeamsService _proTeamsService;
    private ProPlayersService _proPlayersService;
    private HeroService _heroService;
    private StringBuilder _sb;
    private Dictionary<long, UserSession> _userSessions;
   

    public TelegramBotService(Logger logger, ProPlayersService proPlayersService, ProTeamsService proTeamsService, HeroService heroService)
    {
        _userSessions = new Dictionary<long, UserSession>();
        _sb = new StringBuilder();
        _logger = logger;
        _proPlayersService = proPlayersService;
        _proTeamsService = proTeamsService;
        _heroService = heroService;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("apiconfig.json")
            .Build();

        string apiKey = configuration["ApiSettings:ApiKey"];

        if (string.IsNullOrEmpty(apiKey))
        {
            throw new Exception("API key is missing in apiconfig.json");
        }

        _client = new TelegramBotClient(apiKey);
        _bot = _client.GetMe().GetAwaiter().GetResult();
    }

    public void StartTelegramBot()
    {
        Console.WriteLine($"Hello, World! I am user {_bot.Id} and my name is {_bot.FirstName}.");

        _client.OnMessage += TelegramClient_OnMessage;
        _client.OnError += TelegramClient_OnError;
        _client.OnUpdate += TelegramClient_OnUpdate;
    }

    private async Task TelegramClient_OnUpdate(Update update)
    {
        var chatId = update.CallbackQuery.Message.Chat.Id;
        try
        {
            if (!_userSessions.ContainsKey(chatId))
            {
                _userSessions[chatId] = new UserSession();
            }

            var session = _userSessions[chatId];

            if (update.CallbackQuery is { } query)
            {
                await _client.AnswerCallbackQuery(query.Id);

                switch (query.Data)
                {
                    case "show_hero_menu":
                        await ShowHeroMenu(chatId);
                        break;
                    case "show_pro_players_menu":
                        await ShowProPlayersMenu(chatId);
                        break;
                    case "show_pro_teams_menu":
                        await ShowProTeamsMenu(chatId);
                        break;
                    case "back_to_main":
                        session.UserStatus = "null";
                        session.CreataeHeroStatus = "null";
                        await ShowMainMenu(chatId);
                        break;
                    case "prev":                        
                    case "next":
                        await MenusHandler(chatId, session, query.Data);
                        break;
                    
                }

                if (query.Data.StartsWith("get_") || query.Data.StartsWith("add_") || query.Data.StartsWith("update_") || query.Data.StartsWith("delete_"))
                {
                    await HeroMenuHandler(chatId, query, session);
                    await ProPlayerMenuHandler(chatId, query, session);
                    await ProTeamsMenuHandler(chatId, query, session);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Update handling error: {ex.Message}");
        }
    }
    private async Task TelegramClient_OnError(Exception exception, HandleErrorSource source)
    {
        _logger.Error($"telegram bot has error: {exception.Message}");
    }
    private async Task TelegramClient_OnMessage(Message msg, UpdateType type)
    {
        var chadId = msg.Chat.Id;
        try
        {
            if (!_userSessions.ContainsKey(msg.Chat.Id))
            {
                _userSessions[msg.Chat.Id] = new UserSession();
            }

            var session = _userSessions[msg.Chat.Id];

            if (msg.Chat.Type == ChatType.Group || msg.Chat.Type == ChatType.Supergroup)
            {
                return;
            }

            if (msg.Chat.Type == ChatType.Private && msg.Text != null && Regex.IsMatch(msg.Text, @"[\u1F600-\u1F64F\u2702\u2705\u2615\u2764\u1F4A9]+"))
            {
                switch (msg.Text)
                {
                    case "/start":
                        await ShowMainMenu(msg.Chat.Id);
                        break;
                }

                switch (session.UserStatus)
                {
                    case "search_hero":
                        try
                        {
                            Console.Write("\nEnter hero id: ");
                            if (!int.TryParse(msg.Text, out int heroId))
                            {
                                await _client.SendMessage(msg.Chat.Id, "Enter valid id!!!");
                                return;
                            }

                            var heroById = await _heroService.GetHeroByIdAsync(heroId);

                            await _client.SendMessage(msg.Chat.Id, $"Hero by id: {heroId}:\n{heroById}", replyMarkup: new InlineKeyboardMarkup(new[]
                                { new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") } }));
                            _logger.Information($"UI show hero {heroId} benchmark");
                            break;
                        }
                        catch (Exception ex)
                        {
                            await _client.SendMessage(chadId,ex.Message);
                            _logger.Error($"{ex.Message}");
                        }
                        break;
                    case "hero_benchmarks":
                        try
                        {
                            if (!int.TryParse(msg.Text, out int heroId))
                            {
                                await _client.SendMessage(msg.Chat.Id, "Enter valid id!!!");
                                _logger.Information("User entered an incorrect hero id");
                                return;
                            }

                            var heroBenchmark = await _heroService.GetHeroBenchmarcksAsync(heroId);
                            var heroById = await _heroService.GetHeroByIdAsync(heroId);

                            await _client.SendMessage(msg.Chat.Id, $"{heroById.Name} benchmarks:\n{heroBenchmark}", replyMarkup: new InlineKeyboardMarkup(new[]
                                { new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") } }));
                            _logger.Information($"UI show hero {heroId} benchmark");
                        }
                        catch (Exception ex)
                        {
                            await _client.SendMessage(chadId, ex.Message);
                            _logger.Error($"{ex.Message}");
                        }
                        break;
                    case "hero_add":
                        try
                        {
                            if(session.TempHero == null)
                            {
                                session.CreataeHeroStatus = "hero_create_name";
                                return;
                            }
                            var result = await _heroService.PostHeroAsync(session.TempHero);

                            await _client.SendMessage(chadId, result, replyMarkup: new InlineKeyboardMarkup(new[]
                                { new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") } }));
                            _logger.Information($"UI addet hero by id: {session.TempHero.Id}");
                        }
                        catch (Exception ex)
                        {
                            await _client.SendMessage(chadId, ex.Message, replyMarkup: new InlineKeyboardMarkup(new[]
                                { new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") } }));
                            _logger.Error($"{ex.Message}");
                        }
                        break;
                    case "hero_update":
                        try
                        {
                            if (session.TempHero == null)
                            {
                                session.CreataeHeroStatus = "hero_create_name";
                                session.IsUpdate = true;
                                return;
                            }

                            var result = await _heroService.PutHeroAsync(session.TempHero);

                            await _client.SendMessage(chadId, result, replyMarkup: new InlineKeyboardMarkup(new[]
                                { new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") } }));
                            _logger.Information($"UI updated hero by id: {session.TempHero.Id}");
                        }
                        catch (Exception ex)
                        {
                            await _client.SendMessage(chadId, ex.Message);
                            _logger.Error($"{ex.Message}");
                        }
                        break;
                    case "hero_delete":
                        try
                        {
                            if (!int.TryParse(msg.Text, out int heroId))
                            {
                                await _client.SendMessage(msg.Chat.Id, "Enter valid id!!!");
                                _logger.Information("User entered an incorrect hero id");
                                return;
                            }

                            var result = await _heroService.DeleteHeroAsync(heroId);

                            await _client.SendMessage(msg.Chat.Id, result, replyMarkup: new InlineKeyboardMarkup(new[]
                                { new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") } }));

                            _logger.Information($"UI deleted hero by id: {heroId}");
                        }
                        catch (Exception ex)
                        {
                            await _client.SendMessage(chadId, ex.Message);
                            _logger.Error($"{ex.Message}");
                        }
                        break;
                }
                switch (session.CreataeHeroStatus)
                {
                    case "hero_create_name":
                        session.TempHero = new Hero();
                        session.TempHero.Name = msg.Text.Trim();

                        if (string.IsNullOrWhiteSpace(session.TempHero.Name))
                        {
                            _logger.Error("Empty hero name entered.");
                            await _client.SendMessage(msg.Chat.Id, "Hero name cannot be empty. Try again: ");
                            return;
                        }

                        if (session.IsUpdate)
                        {
                            session.CreataeHeroStatus = "hero_update_id";
                            await _client.SendMessage(msg.Chat.Id, "Enter hero id: ");
                        }
                        else
                        {
                            session.CreataeHeroStatus = "hero_create_attribute";
                            await _client.SendMessage(msg.Chat.Id, "Enter primary attribute (str/agi/int): ");
                        }

                        break;
                    case "hero_update_id":
                        if(!int.TryParse(msg.Text, out int id))
                        {
                            _logger.Error("Empty hero name entered.");
                            await _client.SendMessage(msg.Chat.Id, "Enter valid hero id. Try again: ");
                            return;
                        }

                        session.CreataeHeroStatus = "hero_create_attribute";
                        await _client.SendMessage(msg.Chat.Id, "Enter primary attribute (str/agi/int): ");

                        break;
                    case "hero_create_attribute":
                        session.TempHero.Attribute = msg.Text.Trim();

                        if(session.TempHero.Attribute != "str" && session.TempHero.Attribute != "agi" && session.TempHero.Attribute != "int")
                        {
                            _logger.Error($"Invalid primary attribute entered: {session.TempHero.Attribute}");
                            await _client.SendMessage(msg.Chat.Id, "Invalid attribute. Enter 'str', 'agi' or 'int': ");
                            return;
                        }

                        session.CreataeHeroStatus = "hero_create_attack_type";
                        await _client.SendMessage(msg.Chat.Id, "Enter attack type (Melee/Range): ");
                        break;
                    case "hero_create_attack_type":
                        session.TempHero.AttackType = msg.Text.Trim();

                        if (!string.Equals(session.TempHero.AttackType, "Melee", StringComparison.OrdinalIgnoreCase) &&
                               !string.Equals(session.TempHero.AttackType, "Range", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.Error($"Invalid attack type entered: {session.TempHero.AttackType}");
                            await _client.SendMessage(msg.Chat.Id, "Invalid attack type. Enter 'Melee' or 'Range': ");
                            return;
                        }
                        session.TempHero.AttackType = Capitalize(session.TempHero.AttackType);

                        session.CreataeHeroStatus = "hero_create_legs";
                        await _client.SendMessage(msg.Chat.Id, "Enter number of legs: ");
                        break;
                    case "hero_create_legs":
                       
                        if (!int.TryParse(msg.Text, out int legs) || legs < 0 || legs > 8)
                        {
                            _logger.Error($"Invalid number of legs entered: {msg.Text}");
                            await _client.SendMessage(msg.Chat.Id, "Invalid number of legs. Enter a number between 0 and 8: ");
                            return;
                        }

                        session.CreataeHeroStatus = "hero_create_roles";
                        await _client.SendMessage(msg.Chat.Id, "Enter roles (comma separated, e.g. Carry,Support): ");
                        break;
                    case "hero_create_roles":
                        var validRoles = new List<string>
                        {
                            "Carry", "Support", "Nuker", "Disabler",
                            "Jungler", "Durable", "Escape", "Pusher", "Initiator"
                        };

                        var rolesInput = msg.Text.Trim();

                        List<string> roles = rolesInput.Split(',')
                            .Select(x => x.Trim())
                            .Where(r => !string.IsNullOrWhiteSpace(r))
                            .ToList();

                        if (roles.Count == 0 || roles.Any(r => !validRoles.Contains(Capitalize(r), StringComparer.OrdinalIgnoreCase)))
                        {
                            _logger.Error($"Invalid or missing roles entered: {rolesInput}");
                            await _client.SendMessage(msg.Chat.Id, "Invalid roles. Enter roles again:");
                            return;
                        }

                        roles = rolesInput.Split(',')
                            .Select(x => x.Trim())
                            .Where(ч => !string.IsNullOrWhiteSpace(ч))
                        .ToList();

                        roles = roles.Select(Capitalize).ToList();
                        session.TempHero.Roles = roles;
                        await _client.SendMessage(chadId, "Hero created successfully!", replyMarkup: new InlineKeyboardMarkup(new[]
                                { new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") } }));
                        session.UserStatus = "null";
                        break;
                }
            }
            else if (msg.Animation != null)
            {
                string animation = msg.Animation.FileId;
                await _client.SendAnimation(msg.Chat.Id, animation);
            }
            else if (msg.Sticker != null)
            {
                string sticker = msg.Sticker.FileId;
                await _client.SendSticker(msg.Chat.Id, sticker);
            }
            else
            {
                await _client.SendMessage(msg.Chat.Id, "I only accept text messages!");
            }
        }
        catch (Exception ex)
        {
            _logger.Error($"Update handling error: {ex.Message}");
        }
        
    }

    private async Task MenusHandler(long chatId, UserSession session, string data)
    {
        if (data.Equals("prev"))
        {
            session.Page = Math.Max(1, session.Page - 1);
        }
        else if (data.Equals("next"))
        {
            session.Page++;
        }

        switch (session.UserStatus)
        {
            case "pro_players":
                await ShowProPlayers(chatId, session.Page);
                break;
            case "pro_teams_and_Fhero":
                await ShowProTeamsAndFavoriteHero(chatId, session.Page);
                break;
            case "pro_teams":
                await ShowProTeam(chatId, session.Page);
                break;
            case "all_hero":
                await ShowAllHero(chatId, session.Page);
                break;
        }

    }
    private async Task HeroMenuHandler(long chatId, CallbackQuery query,UserSession session)
    {
        switch (query.Data)
        {
            case "get_all_hero":
                session.Page = 1;
                session.CreataeHeroStatus = "null";
                session.UserStatus = "all_hero";
                await ShowAllHero(chatId, session.Page);
                break;

            case "get_hero_by_id":
                session.CreataeHeroStatus = "null";
                session.UserStatus = "search_hero";
                await _client.SendMessage(chatId, "Enter hero id for search:");
                break;

            case "get_hero_benchmarks":
                session.CreataeHeroStatus = "null";
                session.UserStatus = "hero_benchmarks";
                await _client.SendMessage(chatId, "Enter hero id for search:");
                break;

            case "add_hero":
                session.UserStatus = "null";
                session.CreataeHeroStatus = "hero_create_name";
                await _client.SendMessage(chatId, "Enter hero name:");
                break;

            case "update_hero":
                session.UserStatus = "null";
                session.CreataeHeroStatus = "hero_create_name";
                session.IsUpdate = true;
                await _client.SendMessage(chatId, "Enter hero name:");
                break;

            case "delete_hero":
                session.UserStatus = "hero_delete";
                session.CreataeHeroStatus = "null";
                await _client.SendMessage(chatId, "Enter hero id to remove:");
                break;
        }
    }
    private async Task ProPlayerMenuHandler(long chatId, CallbackQuery query, UserSession session)
    {
        switch (query.Data)
        {
            case "get_pro_players":
                session.CreataeHeroStatus = "null";
                session.UserStatus = "pro_players";
                session.Page = 1;
                await ShowProPlayers(chatId, session.Page);
                break;
        }
    }
    private async Task ProTeamsMenuHandler(long chatId, CallbackQuery query, UserSession session)
    {
        switch (query.Data)
        {
            case "get_pro_teams":
                session.CreataeHeroStatus = "null";
                session.UserStatus = "pro_teams";
                session.Page = 1;
                await ShowProTeam(chatId, session.Page);
                break;

            case "get_pro_teams_and_fHero":
                session.Page = 1;
                session.CreataeHeroStatus = "null";
                session.UserStatus = "pro_teams_and_Fhero";
                await ShowProTeamsAndFavoriteHero(chatId, session.Page);
                break;
        }
    }

    private async Task ShowHeroMenu(long chatId)
    {
        await _client.SendMessage(chatId, "Choose option:", replyMarkup: new InlineKeyboardMarkup(new[]{
                new[] {InlineKeyboardButton.WithCallbackData("Get all hero", "get_all_hero"),InlineKeyboardButton.WithCallbackData("Get hero for id", "get_hero_by_id") },
                new[] {InlineKeyboardButton.WithCallbackData("Get hero benchmarks", "get_hero_benchmarks") },
                new[] {InlineKeyboardButton.WithCallbackData("Add hero", "add_hero"),InlineKeyboardButton.WithCallbackData("Update hero", "update_hero"),InlineKeyboardButton.WithCallbackData("Delete hero", "delete_hero") },
                new[] {InlineKeyboardButton.WithCallbackData("Back", "back_to_main") }
                }));
    }
    private async Task ShowProTeamsMenu(long chatId)
    {
        await _client.SendMessage(chatId, "Choose option:", replyMarkup: new InlineKeyboardMarkup(new[]{
                new[] {InlineKeyboardButton.WithCallbackData("Get pro teams", "get_pro_teams") },
                new[] {InlineKeyboardButton.WithCallbackData("Get pro teams and favorite hero", "get_pro_teams_and_fHero") },
                new[] {InlineKeyboardButton.WithCallbackData("Back", "back_to_main") }
                }));
    }
    private async Task ShowProPlayersMenu(long chatId)
    {
        await _client.SendMessage(chatId, "Choose option:", replyMarkup: new InlineKeyboardMarkup(new[]{
                new[] {InlineKeyboardButton.WithCallbackData("Get pro players", "get_pro_players") },
                new[] {InlineKeyboardButton.WithCallbackData("Back", "back_to_main")}
        }));
    }
    private async Task ShowMainMenu(long chatId)
    {
        await _client.SendMessage(chatId, "Hello, choose option:", replyMarkup: new InlineKeyboardMarkup(new[]{
                new[] {InlineKeyboardButton.WithCallbackData("Hero menu","show_hero_menu") },
                new[] {InlineKeyboardButton.WithCallbackData("Pro players menu", "show_pro_players_menu") },
                new[] {InlineKeyboardButton.WithCallbackData("Pro teams menu", "show_pro_teams_menu") }
                }));
    }

    private async Task ShowProPlayers(long chatId, int page)
    {
        try
        {
            _sb.Clear();
            _sb.AppendLine("All pro players:");
            _sb.AppendLine($"Page: {page}\n");


            var proPlayers = await _proPlayersService.GetProPlayersAsync(10, page);

            foreach (var player in proPlayers)
            {
                _sb.Append($"{player}\n");
            }
            if (proPlayers.Count == 0)
            {
                await _client.SendMessage(chatId, "No more pro players...", linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                        new[] { InlineKeyboardButton.WithCallbackData("Prev", "prev")},
                        new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") }
                }));
                return;
            }
            await _client.SendMessage(chatId, _sb.ToString(), linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                        new[] { InlineKeyboardButton.WithCallbackData("Prev", "prev"), InlineKeyboardButton.WithCallbackData("Next", "next") },
                        new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") }
            }));

            _logger.Information($"UI show pro player");
        }
        catch (Exception ex)
        {
            _logger.Error($"{ex.Message}");
        }
    }
    private async Task ShowProTeam(long chatId, int page)
    {
        try
        {
            _sb.Clear();
            _sb.AppendLine("All pro teams:");
            _sb.AppendLine($"Page: {page}\n");

            var proTeams = await _proTeamsService.GetProTeamsAsync(10, page);

            foreach (var teams in proTeams)
            {
                _sb.AppendLine($"{teams}\n");
            }
            if (proTeams.Count == 0)
            {
                await _client.SendMessage(chatId, "No more teams...", linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                        new[] { InlineKeyboardButton.WithCallbackData("Prev", "prev")},
                        new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") }
                }));
                return;
            }
            await _client.SendMessage(chatId, _sb.ToString(), linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                        new[] { InlineKeyboardButton.WithCallbackData("Prev", "prev"), InlineKeyboardButton.WithCallbackData("Next", "next") },
                        new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") }
            }));
            _logger.Information($"UI show pro teams");
        }
        catch (Exception ex)
        {
            _logger.Error($"{ex.Message}");
        }
    }
    private async Task ShowProTeamsAndFavoriteHero(long chatId, int page)
    {
        try
        {
            _sb.Clear();
            _sb.AppendLine("All pro teams and favorite hero:");
            _sb.AppendLine($"Page: {page}\n");

            var proTeamsF = await _proTeamsService.GetProTeamsAndFavoriteHeroAsync(10,page);
            foreach (var teams in proTeamsF)
            {
                _sb.Append($"{teams}\n");
            }

            if (proTeamsF.Count == 0)
            {
                await _client.SendMessage(chatId, "No more teams...", linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                        new[] { InlineKeyboardButton.WithCallbackData("Prev", "prev")},
                        new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") }
                }));
                return;
            }
            await _client.SendMessage(chatId, _sb.ToString(), linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                        new[] { InlineKeyboardButton.WithCallbackData("Prev", "prev"), InlineKeyboardButton.WithCallbackData("Next", "next") },
                        new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") }
            }));

            _logger.Information($"UI show pro teams");
        }
        catch (Exception ex)
        {
            _logger.Error($"{ex.Message}");
        }
    }
    private async Task ShowAllHero(long chatId, int page)
    {
        var limit = 10;
        try
        {
            var heroes = await _heroService.GetAllHeroAsync();
            var paginationHeroes =  heroes.Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            _sb.Clear();
            _sb.AppendLine("All heroes:");
            _sb.AppendLine($"Page: {page}\n");

            foreach (var hero in paginationHeroes)
            {
                _sb.Append($"{hero}\n");
                Console.WriteLine(hero);
            }

            if (paginationHeroes.Count == 0)
            {
                await _client.SendMessage(chatId, "No more heroes...", linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
                replyMarkup: new InlineKeyboardMarkup(new[]
                {
                        new[] { InlineKeyboardButton.WithCallbackData("Prev", "prev")},
                        new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") }
                }));
                return;
            }

            await _client.SendMessage(chatId, _sb.ToString(), linkPreviewOptions: new LinkPreviewOptions { IsDisabled = true },
            replyMarkup: new InlineKeyboardMarkup(new[]
            {
                        new[] { InlineKeyboardButton.WithCallbackData("Prev", "prev"), InlineKeyboardButton.WithCallbackData("Next", "next") },
                        new[] { InlineKeyboardButton.WithCallbackData("Back to menu", "back_to_main") }
            }));

            _logger.Information("UI show all heroes");
        }
        catch (Exception ex)
        {
            _logger.Error($"{ex.Message}");
        }
    }

    private string Capitalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        return char.ToUpper(input[0]) + input.Substring(1).ToLower();
    }
}
