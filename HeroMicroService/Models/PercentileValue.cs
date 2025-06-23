using System.Text.Json.Serialization;

namespace UI.Models;

public class PercentileValue
{
    [JsonPropertyName("percentile")]
    public double Percentile { get; set; }

    [JsonPropertyName("value")]
    public double? Value { get; set; }
}
