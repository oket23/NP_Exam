using HeroMicroService.Models;
using System;

namespace HeroMicroService.Services;

public static class HeroValidator
{
    public static bool ValidHero(Hero hero)
    {
        return hero != null && 
            ValidName(hero.Name) &&
            ValidAttr(hero.Attribute) &&
            ValidAttackType(hero.AttackType) &&
            ValidRoles(hero.Roles) &&
            ValidLegs(hero.Legs);
    }

    private static bool ValidLegs(int legs)
    {
        return legs > 0;
    }

    private static bool ValidRoles(List<string> roles)
    {
        return roles != null && roles.Count >= 1 && roles.All(r => !string.IsNullOrWhiteSpace(r));
    }

    private static bool ValidAttackType(string attackType)
    {
        if (attackType == null)
        {
            return false;
        }
        return !string.IsNullOrWhiteSpace(attackType);
    }

    private static bool ValidAttr(string attribute)
    {
        if (attribute == null)
        {
            return false;
        }
        return !string.IsNullOrWhiteSpace(attribute);
    }

    private static bool ValidName(string name)
    {
        if(name == null)
        {
            return false;
        }
        return !string.IsNullOrWhiteSpace(name) && name.Length <= 32;
    }

}
