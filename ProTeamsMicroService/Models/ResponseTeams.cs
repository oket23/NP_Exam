using System.Text.Json.Serialization;

namespace ProTeamsMicroService.Models;

public class ResponseTeam
{
    [JsonPropertyName("teams")]
    public Team Team { get; set; }
    [JsonPropertyName("favorite_hero")]
    public TeamHeroStats FavoriteHero { get; set; }
}
