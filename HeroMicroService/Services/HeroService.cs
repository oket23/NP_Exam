using HeroMicroService.Models;
using Microsoft.Extensions.Caching.Memory;
using Serilog.Core;
using System.Text.Json;
using UI.Models;

namespace HeroMicroService.Services;

public class HeroService
{
    private HeroApiService _heroApi;
    private HeroDbService _heroDb;
    private Logger _logger;
    private MemoryCache _cache;
    private readonly HeroContext _heroContext;
    private HttpClient _httpClient;

    public HeroService(Logger logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _heroContext = new HeroContext();
        _heroDb = new HeroDbService(logger, _heroContext);
        _heroApi = new HeroApiService(logger, _httpClient);
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public async Task<List<Hero>> GetAllHeroAsync()
    {
        var allHeroesDb = await _heroDb.GetAllHeroAsync();
        _logger.Information("HeroService got all heroes from DB");

        var key = "allHero:cache";
        var allHeroesApi = new List<Hero>();

        if (!_cache.TryGetValue(key, out string json))
        {
            allHeroesApi = await _heroApi.GetAllHeroAsync();
            json = JsonSerializer.Serialize(allHeroesApi);
            _cache.Set(key, json, TimeSpan.FromMinutes(15));
            _logger.Information("HeroService got all heroes from API");
        }
        else
        {
            _logger.Information("HeroService got all heroes from cache");
            allHeroesApi = JsonSerializer.Deserialize<List<Hero>>(json);
        }

        _logger.Information("HeroService returning merged hero list");
        return allHeroesApi.Concat(allHeroesDb).ToList();
    }

    public async Task<Hero> GetHeroByIdAsync(int heroId)
    {
        var result = new Hero();
        var key = $"hero:{heroId}";

        if (!_cache.TryGetValue(key, out string json))
        {
            result = await _heroApi.GetHeroByIdAsync(heroId);
            json = JsonSerializer.Serialize(result);
            _cache.Set(key, json, TimeSpan.FromMinutes(15));

            _logger.Information($"HeroService hero with id:{heroId} loaded from API");
        }
        else
        {
            result = JsonSerializer.Deserialize<Hero>(json)!;
            _logger.Information($"HeroService hero with id:{heroId} loaded from cache");
        }

        _logger.Information($"HeroService returning hero with id:{heroId}");
        return result;
    }
    public async Task<HeroStatsWrapper> GetHeroBenchmarkAsync(int heroId)
    {
        var key = $"benchmark:{heroId}";
        var result = new HeroStatsWrapper();

        if (!_cache.TryGetValue(key, out string json))
        {
            result = await _heroApi.GetHeroBenchmarksAsync(heroId);
            json = JsonSerializer.Serialize(result);
            _cache.Set(key, json, TimeSpan.FromMinutes(15));

            _logger.Information($"HeroService benchmark for hero {heroId} loaded from API");
        }
        else
        {
            result = JsonSerializer.Deserialize<HeroStatsWrapper>(json);
            _logger.Information($"HeroService benchmark for hero {heroId} loaded from cache");
        }

        _logger.Information($"HeroService returning benchmarks hero with id:{heroId}");
        return result;
    }

    public async Task AddHeroAsync(Hero hero)
    {
        await _heroDb.AddHeroAsync(hero);
        _logger.Information($"HeroService addet hero {hero.Id} to DB");
    }
    public async Task UpdateHeroAsync(Hero hero)
    {
        await _heroDb.UpdateHeroAsync(hero);
        _logger.Information($"HeroService updated hero {hero.Id} in DB");
    }
    public async Task DeleteHeroAsync(int id)
    {
        await _heroDb.DeleteHeroAsync(id);
        _logger.Information($"HeroService deleted hero {id} from DB");
    }

    public async Task<bool> IsIdValid(int id)
    {
        var json = await File.ReadAllTextAsync("../../../heroesIds.json");
        var idArray = JsonSerializer.Deserialize<IdArray>(json);

        return idArray.valid_ids.Contains(id);
    }
}
