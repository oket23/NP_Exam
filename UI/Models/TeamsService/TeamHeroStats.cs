using System.Text.Json.Serialization;

namespace ProTeamsMicroService.Models;

public class TeamHeroStats
{
    [JsonPropertyName("hero_id")]
    public int Id { get; set; }
    [JsonPropertyName("localized_name")]
    public string Name { get; set; }
    [JsonPropertyName("games_played")]
    public int Games { get; set; }
    [JsonPropertyName("wins")]
    public int Wins { get; set; }
}
