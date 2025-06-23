using HeroMicroService.Models;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;
using System.Net;
using System.Text;
using System.Text.Json;

namespace HeroMicroService.Services;

public class ListenerService
{
    private HttpListener _listener;
    private HeroService _heroService;
    private Logger _logger;

    public ListenerService(Logger logger, HeroService heroService)
    {
        _logger = logger;
        _heroService = heroService;

        _listener = new HttpListener();
        _listener.Prefixes.Add("http://localhost:8000/");
        _listener.Start();

        _logger.Information("Server started!");
        Console.WriteLine("Server started!");
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

    public async Task ListenerHandlerAsync()
    {
        while (true)
        {
            var httpContext = _listener.GetContext();

            var method = httpContext.Request.HttpMethod;
            var localPath = httpContext.Request.Url.LocalPath;

            var request = httpContext.Request;
            var response = httpContext.Response;

            _logger.Information($"Gets Endpoints: {localPath}");
            Console.WriteLine($"Gets Endpoints: {localPath} on {DateTime.UtcNow}");

            await MethodHandlerAsync(method, localPath, response,request);

            response.Close();
        }
    }

    private async Task MethodHandlerAsync(string method, string localPath, HttpListenerResponse response,HttpListenerRequest request)
    {
        switch (method)
        {
            default:
                _logger.Error($"Cloud not found endpoint {localPath}");
                SendResponse(response, "Incorrect http method!", 501);
                break;

            case "GET":

                if (localPath == "/heroes")
                {
                    try
                    {
                        var heroes = await _heroService.GetAllHeroAsync();
                        
                        if (heroes != null)
                        {
                            var json = JsonSerializer.Serialize(heroes);
                            SendResponse(response, json, 200);
                            _logger.Information("Heroes successfully returning");
                            return;
                        }

                        _logger.Error("Heroes not found");
                        SendResponse(response, "Heroes not found", 404);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error($"{ex.Message}");
                        SendResponse(response, ex.Message, 400);
                    }
                }
                else if (localPath.StartsWith("/heroes/"))
                {
                    try
                    {
                        var idSting = localPath.TrimStart("/heroes/".ToArray());

                        if (!int.TryParse(idSting, out int heroId))
                        {
                            _logger.Error($"Invalid hero id: {idSting}");
                            SendResponse(response, $"Invalid hero id: {idSting}", 400);
                            return;
                        }

                        var hero = await _heroService.GetHeroByIdAsync(heroId);

                        if (hero != null)
                        {
                            var json = JsonSerializer.Serialize(hero);
                            SendResponse(response, json, 200);
                            _logger.Information($"Hero with id: {hero.Id} successfully returning");
                            return;
                        }

                        _logger.Error($"Hero with id {heroId} not found");
                        SendResponse(response, $"Hero with id {heroId} not found", 404);
                    }
                    catch(Exception ex)
                    {
                        _logger.Error(ex, $"{ex.Message}");
                        SendResponse(response, ex.Message, 400);
                    }  
                }
                else if (localPath.StartsWith("/benchmarks/"))
                {
                    try
                    {
                        var idSting = localPath.TrimStart("/benchmarks/".ToArray());

                        if (!int.TryParse(idSting, out int heroBId))
                        {
                            _logger.Error($"Invalid hero id: {idSting}");
                            SendResponse(response, $"Invalid hero id: {idSting}", 400);
                            return;
                        }

                        if (!await _heroService.IsIdValid(heroBId))
                        {
                            _logger.Error($"Benchmarks by hero with id {heroBId} not found");
                            SendResponse(response, $"Benchmarks by hero with id {heroBId} not found", 404);
                            return;
                        }

                        var benchmarks = await _heroService.GetHeroBenchmarkAsync(heroBId);

                        var json = JsonSerializer.Serialize(benchmarks);
                        _logger.Information($"Hero benchmarks successfully returning with id: {heroBId}");
                        SendResponse(response, json, 200);

                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"{ex.Message}");
                        SendResponse(response, ex.Message, 400);
                    }

                }
                else
                {
                    _logger.Error($"Cloud not found endpoint {localPath}");
                    SendResponse(response, $"Cloud not found endpoint  {localPath}", 404);
                }
            break;  

            case "POST":
                if (localPath == "/heroes")
                {
                    try
                    {
                        string requestBody;
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            requestBody = await reader.ReadToEndAsync();
                        }

                        var hero = JsonSerializer.Deserialize<Hero>(requestBody);

                        if (hero != null)
                        {
                            await _heroService.AddHeroAsync(hero);

                            _logger.Information($"Hero successfully added with id: {hero.Id}");
                            SendResponse(response, $"Hero successfully added with id: {hero.Id}", 200);
                            return;
                        }

                        _logger.Error($"Invalid hero data");
                        SendResponse(response, "Invalid hero data", 400);
                    }
                    catch (DbUpdateException)
                    {
                        _logger.Error("Error while adding hero to DB, id and name must be unique!");
                        SendResponse(response, "Error while adding hero to DB, id and name must be unique!", 400);
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

            case "PUT":

                if (localPath == "/heroes")
                {
                    try
                    {
                        string requestBody;
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            requestBody = await reader.ReadToEndAsync();
                        }
                        
                        var hero = JsonSerializer.Deserialize<Hero>(requestBody);

                        if (hero != null)
                        {
                            await _heroService.UpdateHeroAsync(hero);

                            _logger.Information($"Updated hero with id: {hero.Id}");
                            SendResponse(response, $"Hero successfully updated with id: {hero.Id}", 200);
                            return;
                        }

                        _logger.Error($"Invalid hero data");
                        SendResponse(response, "Invalid hero data", 400);
                        
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"{ex.Message}");
                        SendResponse(response, ex.Message, 400);
                    }
                }
                else
                {
                    _logger.Error($"Cloud not found endpoint {localPath}");
                    SendResponse(response, $"Cloud not found endpoint {localPath}", 404);
                }
                break;

            case "DELETE":

                if (localPath.StartsWith("/heroes/"))
                {
                    try
                    {
                        var idString = localPath.TrimStart("/heroes/".ToCharArray());

                        if (!int.TryParse(idString, out int heroId))
                        {
                            _logger.Error($"Invalid hero id: {idString}");
                            SendResponse(response, $"Invalid hero id: {idString}", 400);
                            return;
                            
                        }

                        await _heroService.DeleteHeroAsync(heroId);

                        _logger.Information($"Hero with id {heroId} deleted.");
                        SendResponse(response, $"Hero with id {heroId} successfully deleted.", 200);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, $"{ex.Message}");
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
}
