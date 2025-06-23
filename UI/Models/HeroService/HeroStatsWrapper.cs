using System.Text;
using System.Text.Json.Serialization;

namespace UI.Models.HeroService;

public class HeroStatsWrapper
{
    [JsonPropertyName("hero_id")]
    public int HeroId { get; set; }

    [JsonPropertyName("result")]
    public HeroStatsResult Result { get; set; }

    public override string ToString()
    {
        var sb = new StringBuilder();

        AppendMetric(sb, "Gold Per Minute", Result.GoldPerMin);
        AppendMetric(sb, "XP Per Minute", Result.XpPerMin);
        AppendMetric(sb, "kills Per Minute", Result.KillsPerMin);
        AppendMetric(sb, "Last Hits Per Minute", Result.LastHitsPerMin);
        AppendMetric(sb, "Hero Damage Per Minute", Result.HeroDamagePerMin);
        AppendMetric(sb, "Hero Healing Per Minute", Result.HeroHealingPerMin);
        AppendMetric(sb, "Tower Damage", Result.TowerDamage);

        return sb.ToString();
    }

    private void AppendMetric(StringBuilder sb, string name, List<PercentileValue> values)
    {
        sb.AppendLine($"\n{name}:");

        foreach (var item in values)
        {
            sb.AppendLine($"{item.Percentile} => {item.Value}");
        }
    }
}
