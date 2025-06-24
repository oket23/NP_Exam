using UI.Models.HeroService;

namespace Exam.Models;

public class UserSession
{
    public string UserStatus { get; set; } = "null";
    public string CreataeHeroStatus = "null";
    public int Page { get; set; } = 1;
    public bool IsUpdate { get; set; }
    public Hero TempHero { get; set; }
}
