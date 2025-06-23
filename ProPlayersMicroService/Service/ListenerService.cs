using Serilog.Core;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ProPlayersMicroService.Service;

public class ListenerService
{
    private Logger _logger;
    private ProPlayersService _service;
    private HttpListener _listener;

    public ListenerService(Logger logger, ProPlayersService service)
    {
        _logger = logger;
        _service = service;

        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:8001/");
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

            _logger.Information($"Gets Endpoints: {localPath}");
            Console.WriteLine($"Gets Endpoints: {localPath} on {DateTime.UtcNow}");

            await MethodHandlerAsync(method, localPath, response, request);

            response.Close();
        }
    }

    private async Task MethodHandlerAsync(string method, string localPath,HttpListenerResponse response, HttpListenerRequest request)
    {
        switch (method)
        {
            default:
                _logger.Error($"Cloud not found endpoint {localPath}");
                SendResponse(response, "Incorrect http method!", 501);
                break;

            case "GET":
                if (localPath.Equals("/proPlayers"))
                {
                    try
                    {
                        var proPlayers = await _service.GetProPlayersAsync();

                        if(proPlayers != null)
                        {
                            var json = JsonSerializer.Serialize(proPlayers);
                            SendResponse(response, json, 200);

                            _logger.Information("Pro players successfully returning");
                            return;
                        }

                        _logger.Error("Pro players not found");
                        SendResponse(response, "Pro players not found", 404);
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
