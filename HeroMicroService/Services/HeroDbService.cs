using HeroMicroService.Models;
using Microsoft.EntityFrameworkCore;
using Serilog.Core;

namespace HeroMicroService.Services;

public class HeroDbService
{
    private HeroContext _context;
    private Logger _logger;

    public HeroDbService(Logger logger,HeroContext context)
    {
        _logger = logger;
        _context = context;
    }
    public async Task<List<Hero>> GetAllHeroAsync()
    {
        _logger.Information("HeroDbSerice return all hero");
        return await _context.Heroes.ToListAsync();
    }

    public async Task AddHeroAsync(Hero hero)
    {
        if (!HeroValidator.ValidHero(hero))
        {
            _logger.Error("AddHeroAsync invalid hero data");
            throw new ArgumentException("Invalid hero data");
        }

        _context.Heroes.Add(hero);
        await _context.SaveChangesAsync();

        _logger.Information("HeroDbSerice added new hero");
    }

    public async Task UpdateHeroAsync(Hero hero)
    {
        if (!HeroValidator.ValidHero(hero))
        {
            _logger.Error("UpdateHeroAsync invalid hero data update");
            throw new ArgumentException("Invalid hero data");
        }

        var existingHero = await _context.Heroes.FindAsync(hero.Id);
        if (existingHero == null)
        {
            _logger.Error($"Hero with id {hero.Id} not found in DB");
            throw new KeyNotFoundException($"Hero with id {hero.Id} not found in DB");
        }

        existingHero.Name = hero.Name;
        existingHero.Attribute = hero.Attribute;
        existingHero.AttackType = hero.AttackType;
        existingHero.Roles = hero.Roles;
        existingHero.Legs = hero.Legs;

        await _context.SaveChangesAsync();

        _logger.Information($"HeroDbService updated hero with id {hero.Id}");
    }

    public async Task DeleteHeroAsync(int id)
    {
        var hero = await _context.Heroes.FindAsync(id);
        if (hero == null)
        {
            _logger.Error($"Hero with id:{id} was not found");
            throw new InvalidOperationException("Hero not found");
        }
            
        _context.Heroes.Remove(hero);
        await _context.SaveChangesAsync();

        _logger.Information($"HeroDbSerice deleted hero with id:{id}");
    }

}
