using System.Text.Json.Serialization;

namespace ProPlayersMicroService.Models;

public class ProPlayer
{
    [JsonPropertyName("account_id")]
    public int Id { get; set; }
    
    [JsonPropertyName("steamid")]
    public string Steamid { get; set; }

    [JsonPropertyName("profileurl")]
    public string ProfileUrl { get; set; }

    [JsonPropertyName("personaname")]
    public string Name { get; set; }

    [JsonPropertyName("country_code")]
    public string CountryCode { get; set; }
    [JsonPropertyName("name")]
    public string Nickname { get; set; }
    [JsonPropertyName("fantasy_role")]
    public int Role { get; set; }
    [JsonPropertyName("team_id")]
    public int TeamId { get; set; }
    [JsonPropertyName("team_name")]
    public string TeamName { get; set; }
    [JsonPropertyName("team_tag")]
    public string TeamTag { get; set; }
    [JsonPropertyName("is_locked")]
    public bool IsLocked { get; set; }
    [JsonPropertyName("is_pro")]
    public bool IsPro { get; set; }

}
