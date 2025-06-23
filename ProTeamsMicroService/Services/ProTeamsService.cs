using Microsoft.Extensions.Caching.Memory;
using ProTeamsMicroService.Models;
using Serilog.Core;
using System.Text.Json;

namespace ProTeamsMicroService.Services;

public class ProTeamsService
{
    private Logger _logger;
    private HttpClient _client;
    private MemoryCache _cache;

    public ProTeamsService(Logger logger)
    {
        _logger = logger;
        _client = new HttpClient();
        _client.BaseAddress = new Uri("https://api.opendota.com/api/");
        _cache = new MemoryCache(new MemoryCacheOptions());
    }

    public async Task<List<ResponseTeam>> GetProTeamAndFavoriteHeroAsync(int limit = 50,int page = 1)
    {
        var key = $"proTeams:favoriteHero:limit={limit}:page={page}";
        var result = new List<ResponseTeam>();

        if (!_cache.TryGetValue(key, out string json))
        {
            if (limit < 1 || limit > 50 || page < 1 || page > 20)
            {
                _logger.Error("Bad query params.Max limit = 50,max page = 20");
                throw new ArgumentException("Bad query params.Max limit = 50,max page = 20");
            }

            var teams = await GetTeamsAsync();
            var paginationTeams = teams.Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            var tasks = paginationTeams.Select(async team => new ResponseTeam
            {
                Team = team,
                FavoriteHero = await GetFavoriteHeroByTeamAsync(team)
            });

            result = (await Task.WhenAll(tasks)).ToList();
            _cache.Set(key, JsonSerializer.Serialize(result), TimeSpan.FromMinutes(15));
            _logger.Information("ProTeamsService got all pro teams and favorite hero from API");
        }
        else
        {
            _logger.Information("ProTeamsService got all pro teams and favorite hero from cache");
            result = JsonSerializer.Deserialize<List<ResponseTeam>>(json);
        }

        
        _logger.Information($"All teams with favorite hero successfully returned");
        return result;
    }

    private async Task<TeamHeroStats> GetFavoriteHeroByTeamAsync(Team team)
    {
        var heroStats = await GetHeroStatsAsync(team.Id);

        if (heroStats != null)
        {
            var favorite = new TeamHeroStats();
            int maxGames = int.MinValue;

            foreach (var hero in heroStats)
            {
                if (hero.Games > maxGames)
                {
                    maxGames = hero.Games;
                    favorite = hero;
                }
            }

            _logger.Information($"Heroes stats for team with id: {team.Id} successfully returned");
            return favorite;
        }

        _logger.Error($"Heroes stats for team with id: {team.Id} not found");
        throw new HttpRequestException($"Heroes stats for team with id: {team.Id} not found");
    }

    public async Task<List<Team>> GetTeamsAsync()
    {
        var key = $"proTeams:allTeams";

        if (!_cache.TryGetValue(key, out string json))
        {
            var response = await _client.GetAsync("teams");
            var responseContext = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.Error("Failed to got pro teams from API");
                throw new HttpRequestException("Failed to get pro teams from API");
            }

            _logger.Information("ProTeamsService got pro teams from API");
            _cache.Set(key, responseContext, TimeSpan.FromMinutes(15));
            return JsonSerializer.Deserialize<List<Team>>(responseContext);
        }
        else
        {
            _logger.Information("ProTeamsService got all pro teams from cache");
            return JsonSerializer.Deserialize<List<Team>>(json);
        }
        
    }

    private async Task<List<TeamHeroStats>> GetHeroStatsAsync(int teamId)
    {
        var response = await _client.GetAsync($"teams/{teamId}/heroes");
        var responseContext = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error($"Failed to got hero stats for team: {teamId} from API");
            throw new HttpRequestException($"Failed to got hero stats for team: {teamId} from API");
        }

        _logger.Information($"ProTeamsService got hero stats for team: {teamId} from API");
        return JsonSerializer.Deserialize<List<TeamHeroStats>>(responseContext);
    }
}
