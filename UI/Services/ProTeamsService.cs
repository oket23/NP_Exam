using ProTeamsMicroService.Models;
using Serilog.Core;
using System.Text.Json;

namespace UI.Services;

public class ProTeamsService
{
    private Logger _logger;
    private HttpClient _httpClient;

    public ProTeamsService(Logger logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:8002/");
    }

    public async Task<List<Team>> GetProTeamsAsync()
    {
        var response = await _httpClient.GetAsync("/pro_teams");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error(responseContent);
            throw new HttpRequestException(responseContent);
        }

        _logger.Information("ProTeamsService returns all pro teams");
        return JsonSerializer.Deserialize<List<Team>>(responseContent);
    }

    public async Task<List<ResponseTeam>> GetProTeamsAndFavoriteHeroAsync()
    {
        var response = await _httpClient.GetAsync("pro_teams/favorite");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error(responseContent);
            throw new HttpRequestException(responseContent);
        }

        _logger.Information("ProTeamsService returns all pro teams and favorite hero");
        return JsonSerializer.Deserialize<List<ResponseTeam>>(responseContent);
    }
}
