using HeroMicroService.Models;
using Serilog.Core;
using System.Text.Json;
using UI.Models;

namespace HeroMicroService.Services;

public class HeroApiService
{
    private HttpClient _client;
    private Logger _logger;

    public HeroApiService(Logger logger,HttpClient client)
    {
        _logger = logger;
        _client = client;
        _client.BaseAddress = new Uri("https://api.opendota.com/api/");
    }

    public async Task<List<Hero>> GetAllHeroAsync()
    {
        var response = await _client.GetAsync("heroes");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error("Failed to get heroes from API");
            throw new HttpRequestException("Failed to get heroes from API");
        }

        _logger.Information("HeroApiService gets all heroes from Dota 2 api");
        return JsonSerializer.Deserialize<List<Hero>>(responseContent);
    }

    public async Task<Hero> GetHeroByIdAsync(int heroId)
    {
        var allHero = await GetAllHeroAsync();

        _logger.Information($"HeroApiService gets hero with id:{heroId} from Dota 2 api");
        return allHero.FirstOrDefault(x => x.Id.Equals(heroId));
    }

    public async Task<HeroStatsWrapper> GetHeroBenchmarksAsync(int heroId)
    {
        var response = await _client.GetAsync($"benchmarks?hero_id={heroId}");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error("Failed to get hero benchmarks from API");
            throw new HttpRequestException("Failed to get hero benchmarks from API");
        }

        _logger.Information($"HeroApiService gets benchmarck by hero with id:{heroId} from Dota 2 api");
        return JsonSerializer.Deserialize<HeroStatsWrapper>(responseContent);
    }
}
