using Serilog.Core;
using System.Text;
using System.Text.Json;
using UI.Models.HeroService;

namespace UI.Services;

public class HeroService
{
    private HttpClient _httpClient;
    private readonly Logger _logger;

    public HeroService(Logger logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:8000/");
    }

    public async Task<List<Hero>> GetAllHeroAsync()
    {
        var response = await _httpClient.GetAsync("/heroes");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error(responseContent);
            throw new HttpRequestException(responseContent);
        }

        _logger.Information("HeroService returns all heroes");
        return JsonSerializer.Deserialize<List<Hero>>(responseContent);
    }

    public async Task<HeroStatsWrapper> GetHeroBenchmarcksAsync(int heroId)
    {
        var response = await _httpClient.GetAsync($"/benchmarks/{heroId}");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error(responseContent);
            throw new HttpRequestException(responseContent);
        }

        _logger.Information($"HeroService returns hero: {heroId} benchmarks");
        return JsonSerializer.Deserialize<HeroStatsWrapper>(responseContent);
    }
    
    public async Task<Hero> GetHeroByIdAsync(int heroId)
    {
        var response = await _httpClient.GetAsync($"/heroes/{heroId}");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error(responseContent);
            throw new HttpRequestException(responseContent);
        }

        _logger.Information($"HeroService returns hero by id {heroId}");

        return JsonSerializer.Deserialize<Hero>(responseContent);
    }

    public async Task<string> PostHeroAsync(Hero hero)
    {
        var json = JsonSerializer.Serialize(hero);
        var context = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("/heroes", context);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error(responseContent);
            throw new HttpRequestException(responseContent);
        }

        _logger.Information($"HeroService post hero with id {hero.Id}");
        return "Hero successfully added\n";
    }

    public async Task<string> PutHeroAsync(Hero hero)
    {
        var json = JsonSerializer.Serialize(hero);
        var context = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PutAsync("/heroes", context);

        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error(responseContent);
            throw new HttpRequestException(responseContent);
        }

        _logger.Information($"HeroService put hero with id {hero.Id}");
        return "Hero successfully updated\n";
    }

    public async Task<string> DeleteHeroAsync(int id)
    {
        var response = await _httpClient.DeleteAsync($"/heroes/{id}");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error(responseContent);
            throw new HttpRequestException(responseContent);
        }

        _logger.Information($"HeroService delete hero with id {id}");
        return $"Hero with id {id} successfully deleted";
    }

    public Hero GetValidHero(bool isUpdate)
    {

        _logger.Information("Starting hero creation.");
        var hero = new Hero();
        int userId = 0;

        Console.Write("\nEnter hero name: ");
        var name = Console.ReadLine().Trim();

        while (string.IsNullOrWhiteSpace(name))
        {
            _logger.Error("Empty hero name entered.");
            Console.Write("Hero name cannot be empty. Try again: ");
            name = Console.ReadLine().Trim();
        }

        if (isUpdate)
        {
            Console.Write("Enter hero id: ");
            int id;
            while (!int.TryParse(Console.ReadLine(), out id))
            {
                _logger.Error("Empty hero name entered.");
                Console.Write("Hero name cannot be empty. Try again: ");
            }
            userId = id;
        }

        Console.Write("Enter primary attribute (str/agi/int): ");
        var attr = Console.ReadLine().Trim().ToLower();

        while (attr != "str" && attr != "agi" && attr != "int")
        {
            _logger.Error($"Invalid primary attribute entered: {attr}");
            Console.Write("Invalid attribute. Enter 'str', 'agi' or 'int': ");
            attr = Console.ReadLine().Trim().ToLower();
        }

        Console.Write("Enter attack type (Melee/Range): ");
        var attackTypeInput = Console.ReadLine().Trim();

        while (!string.Equals(attackTypeInput, "Melee", StringComparison.OrdinalIgnoreCase) &&
               !string.Equals(attackTypeInput, "Range", StringComparison.OrdinalIgnoreCase))
        {
            _logger.Error($"Invalid attack type entered: {attackTypeInput}");
            Console.Write("Invalid attack type. Enter 'Melee' or 'Range': ");
            attackTypeInput = Console.ReadLine().Trim();
        }

        var attackType = Capitalize(attackTypeInput);

        int legs;
        Console.Write("Enter number of legs: ");
        string legsInput = Console.ReadLine().Trim();

        while (!int.TryParse(legsInput, out legs) || legs < 0 || legs > 8)
        {
            _logger.Error($"Invalid number of legs entered: {legsInput}");
            Console.Write("Invalid number of legs. Enter a number between 0 and 8: ");
            legsInput = Console.ReadLine().Trim();
        }

        var validRoles = new List<string>
        {
            "Carry", "Support", "Nuker", "Disabler",
            "Jungler", "Durable", "Escape", "Pusher", "Initiator"
        };

        Console.Write("Enter roles (comma separated, e.g. Carry,Support): ");
        var rolesInput = Console.ReadLine().Trim();

        List<string> roles = rolesInput.Split(',')
            .Select(x => x.Trim())
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .ToList();

        while (roles.Count == 0 || roles.Any(r => !validRoles.Contains(Capitalize(r), StringComparer.OrdinalIgnoreCase)))
        {
            _logger.Error($"Invalid or missing roles entered: {rolesInput}");

            Console.Write("Invalid roles\nEnter roles again: ");
            rolesInput = Console.ReadLine().Trim();

            roles = rolesInput.Split(',')
                .Select(x => x.Trim())
                .Where(ч => !string.IsNullOrWhiteSpace(ч))
                .ToList();
        }

        roles = roles.Select(Capitalize).ToList();
        if (isUpdate)
        {
            hero.Id = userId;
        }
        hero.Name = name;
        hero.Attribute = attr;
        hero.AttackType = attackType;
        hero.Legs = legs;
        hero.Roles = roles;

        _logger.Information($"Hero created: {hero.Name}");
        return hero;
    }
    private string Capitalize(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;
        return char.ToUpper(input[0]) + input.Substring(1).ToLower();
    }

}
