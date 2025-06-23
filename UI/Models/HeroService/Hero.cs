using System.Text.Json.Serialization;

namespace UI.Models.HeroService;

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

    public override string ToString()
    {
        return $"Id: {Id}\nName: {Name}\nAttribute {Attribute}\nAttackType {AttackType}\nRoles:\n  {string.Join("\n  ", Roles)}\nLegs: {Legs}\n";
    }
}
