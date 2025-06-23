using Microsoft.Extensions.Caching.Memory;
using ProPlayersMicroService.Models;
using Serilog.Core;
using System.Text.Json;

namespace ProPlayersMicroService.Service;

public class ProPlayersService
{
    private Logger _logger;
    private HttpClient _client;
    private MemoryCache _cache;

    public ProPlayersService(Logger logger)
    {
        _logger = logger;
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://api.opendota.com/api/");
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public async Task<List<ProPlayer>> GetProPlayersAsync()
    {
        var key = "proPlayers:all";

        if(!_cache.TryGetValue(key,out string json))
        {
            var response = await _client.GetAsync("proPlayers");

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Failed to get pro players from API");
                throw new HttpRequestException("Failed to get pro players from API");
            }

            _cache.Set(key, responseContent,TimeSpan.FromMinutes(15));
            _logger.Information("ProPlayersService gets pro players from Dota 2 API");
            return JsonSerializer.Deserialize<List<ProPlayer>>(responseContent);
        }
        else
        {
            _logger.Information("ProPlayersService gets pro players from Dota 2 cache");
            return JsonSerializer.Deserialize<List<ProPlayer>>(json);
        } 
    }

}
