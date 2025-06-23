using System.Text.Json.Serialization;

namespace UI.Models;

public class HeroStatsResult
{
    [JsonPropertyName("gold_per_min")]
    public List<PercentileValue> GoldPerMin { get; set; } = new();

    [JsonPropertyName("xp_per_min")]
    public List<PercentileValue> XpPerMin { get; set; } = new();

    [JsonPropertyName("kills_per_min")]
    public List<PercentileValue> KillsPerMin { get; set; } = new();

    [JsonPropertyName("last_hits_per_min")]
    public List<PercentileValue> LastHitsPerMin { get; set; } = new();

    [JsonPropertyName("hero_damage_per_min")]
    public List<PercentileValue> HeroDamagePerMin { get; set; } = new();

    [JsonPropertyName("hero_healing_per_min")]
    public List<PercentileValue> HeroHealingPerMin { get; set; } = new();

    [JsonPropertyName("tower_damage")]
    public List<PercentileValue> TowerDamage { get; set; } = new();
}
