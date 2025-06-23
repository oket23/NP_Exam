using ProPlayersMicroService.Models;
using Serilog.Core;
using System.Text.Json;

namespace UI.Services;

public class ProPlayersService
{
    private HttpClient _httpClient;
    private readonly Logger _logger;

    public ProPlayersService(Logger logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:8001/");
    }

    public async Task<List<ProPlayer>> GetProPlayersAsync()
    {
        var response = await _httpClient.GetAsync("/proPlayers");
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.Error(responseContent);
            throw new HttpRequestException(responseContent);
        }

        _logger.Information("ProPlayersService returns all pro players");
        return JsonSerializer.Deserialize<List<ProPlayer>>(responseContent);
    }
}
