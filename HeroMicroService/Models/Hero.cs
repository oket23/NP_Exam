using System.Text.Json.Serialization;

namespace HeroMicroService.Models;

public class Hero
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("localized_name")]
    public string Name { get; set; }
    [JsonPropertyName("primary_attr")]
    public string Attribute { get; set; }
    [JsonPropertyName("attack_type")]
    public string AttackType { get; set; }
    [JsonPropertyName("roles")]
    public List<string> Roles { get; set; }
    [JsonPropertyName("legs")]
    public int Legs { get; set; }
}
