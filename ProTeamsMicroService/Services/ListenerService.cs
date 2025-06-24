using Serilog.Core;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

namespace ProTeamsMicroService.Services;

public class ListenerService
{
    private Logger _logger;
    private ProTeamsService _service;
    private HttpListener _listener;

    public ListenerService(Logger logger,ProTeamsService service)
    {
        _service = service;
        _logger = logger;

        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:8002/");
        _listener.Start();

        _logger.Information("Server started!");
        Console.WriteLine("Server started!");
    }

    public async Task ListenerHandlerAsync()
    {
        while (true)
        {
            var httpContext = _listener.GetContext();

            var method = httpContext.Request.HttpMethod;
            var localPath = httpContext.Request.Url.LocalPath;

            var response = httpContext.Response;
            var request = httpContext.Request;

            var rawQuery = httpContext.Request.Url.Query;
            var queryParams = HttpUtility.ParseQueryString(rawQuery);

            int limit = int.TryParse(queryParams["limit"], out var lim) ? lim : 10;
            int page = int.TryParse(queryParams["page"], out var p) ? p : 1;

            _logger.Information($"Gets Endpoints: {localPath}");
            Console.WriteLine($"Gets Endpoints: {localPath} on {DateTime.UtcNow}");

            await MethodHandlerAsync(method, localPath, response, request, limit, page);

            response.Close();
        }
    }

    public async Task MethodHandlerAsync(string method, string localPath, HttpListenerResponse response, HttpListenerRequest request, int limit, int page)
    {
        switch (method)
        {
            default:
                _logger.Error($"Cloud not found endpoint {localPath}");
                SendResponse(response, "Incorrect http method!", 501);
                break;

            case "GET":
                if (localPath.Equals("/pro_teams"))
                {
                    try
                    {
                        var proTeams = await _service.GetTeamsAsync(limit, page);

                        if (proTeams != null)
                        {
                            var json = JsonSerializer.Serialize(proTeams);
                            _logger.Information($"Pro teams successfully returning");
                            SendResponse(response, json, 200);
                            return;
                        }

                        _logger.Error($"Pro teams not found");
                        SendResponse(response, $"Pro teams not found", 404);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"{ex.Message}");
                        SendResponse(response, ex.Message, 400);
                    }
                }
                else if (localPath.Equals("/pro_teams/favorite"))
                {
                    try
                    {
                        var responseTeams = await _service.GetProTeamAndFavoriteHeroAsync(limit, page);

                        if (responseTeams != null)
                        {
                            var json = JsonSerializer.Serialize(responseTeams);
                            _logger.Information($"Pro teams with favorite hero successfully returning");
                            SendResponse(response, json, 200);
                            return;
                        }

                        _logger.Error($"Pro teams with favorite hero not found");
                        SendResponse(response, $"Pro teams with favorite hero not found", 404);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"{ex.Message}");
                        SendResponse(response, ex.Message, 400);
                    }
                }
                else
                {
                    _logger.Error($"Cloud not found endpoint {localPath}");
                    SendResponse(response, $"Cloud not found endpoint {localPath}", 404);
                }
                break;
        }
    }

    private static void SendResponse(HttpListenerResponse response, string json, int statusCode)
    {
        response.StatusCode = statusCode;
        response.ContentType = "application/json";

        using (var stream = response.OutputStream)
        {
            stream.Write(Encoding.UTF8.GetBytes(json));
            stream.Flush();
        }
    }
}
