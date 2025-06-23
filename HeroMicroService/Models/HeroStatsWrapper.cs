using System.Text.Json.Serialization;

namespace UI.Models;

public class HeroStatsWrapper
{
    [JsonPropertyName("hero_id")]
    public int HeroId { get; set; }

    [JsonPropertyName("result")]
    public HeroStatsResult Result { get; set; }


}
