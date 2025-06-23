using System.Text.Json.Serialization;

namespace ProTeamsMicroService.Models;

public class ResponseTeam
{
    [JsonPropertyName("teams")]
    public Team Team { get; set; }
    [JsonPropertyName("favorite_hero")]
    public TeamHeroStats FavoriteHero { get; set; }

    public override string ToString()
    {
        return $"Team id:{Team.Id}\nTeam name: {Team.Name}" +
            $"\nTeam tag: {Team.Tag}\nTeam wins: {Team.Wins}" +
            $"\nFavorite hero:\nHero id{FavoriteHero.Id}" +
            $"\nHero name: {FavoriteHero.Name}" +
            $"\nHero games: {FavoriteHero.Games}" +
            $"\nHero wins: {FavoriteHero.Wins}\n";
    }
}
