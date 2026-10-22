using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_AddNewHero : ComponentLoader
{
    public int loadIndex = -1;
    public TextMeshProUGUI description;
    public TextMeshProUGUI costText;
    public Image backing;
    private Hero lastHero = null;


    public override void PassObject(object obj)
    {
        if(obj is Hero hero)
            LoadHeroDirect(hero);

        else if(obj is HeroClass heroclass) 
            LoadHeroDirect(new(1 + (GameManager.Distance / 2), heroclass)); 

        if (obj == null) gameObject.SetActive(false);
    }
    public void LoadHeroDirect(Hero hero)
    {
        if (hero == null) return;

        hero.RollName();
        hero.AddStartingWeapons();
        hero.team = GameManager.Player.team;

        SetText(hero.heroClass.className);
        SetDescription($"<u>{hero.heroClass.rarity} Rank {hero.heroClass.rank}</u>\n{hero.heroClass.description}");
        SetImageSprite(hero.heroClass.actorImage);

        if (backing) backing.color = BattleConstants.GetBadgeColor(hero.heroClass.rarity);
        if (costText) costText.text = PrintCostBytAvailability(hero.heroClass);

        lastHero = hero;
    }
    public static string PrintCostBytAvailability(HeroClass hero)
    {
        if (!hero) return "SOLD OUT";

        int cost = BattleConstants.GetHeroHireCost(hero);
        string value = cost.ToString("0 Gold");
        string color = GameManager.Inventory.CanSpend(cost) ? "<color=yellow>" : "<color=red>";
        return color + value + "</color>";
    }
    public void SetDescription(string d)
    {
        if (description) description.text = d;
    }

    public void AddHeroToParty(bool print)
    {
        if (lastHero == null) return;

        GameManager.Player.Add(lastHero);

        if (loadIndex < 0) return;

        GameManager.Inventory.SpendMoney(BattleConstants.GetHeroHireCost(lastHero.heroClass));
        GameManager.CurrentEncounter.recruits[loadIndex] = null;

        if (print) BannerArray.PrintReport($"{lastHero} has joined your Guild!");
    }
    private void Start()
    {
        if (loadIndex >= 0 && loadIndex < GameManager.CurrentEncounter.recruits.Count) 
            PassObject(GameManager.CurrentEncounter.recruits[loadIndex]); 
    }
}
