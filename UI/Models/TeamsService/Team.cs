using System.Text.Json.Serialization;

namespace ProTeamsMicroService.Models;

public class Team
{
    [JsonPropertyName("team_id")]
    public int Id { get; set; }
    [JsonPropertyName("wins")]
    public int Wins { get; set; }
    [JsonPropertyName("losses")]
    public int Losses { get; set; }
    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("tag")]
    public string Tag { get; set; }
}
