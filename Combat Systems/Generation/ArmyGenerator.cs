using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "New Army", menuName = "NMD/Encounters/Army Generator")]
public class ArmyGenerator : ScriptableObject
{
    public GameRewards rewards;
    public Team team;
    public Team mirror;
    public int minLevel, maxLevel;
    public UnitGenerator[] possibleLineUps;
    public UnitGenerator[] bosses;

    [Header("Command Card Deck")]
    public CommandCard[] commands;

    public Team GetTeam(bool _ = false) => team;
    public Unit GetRandomUnit()
    {
        if (possibleLineUps == null || possibleLineUps.Length == 0) return null;

        Unit pull = possibleLineUps[Random.Range(0, possibleLineUps.Length)].Copy(team);

        return ScaleUnit(pull);
    }
    public Unit GetRandomBoss()
    {
        if (bosses == null || bosses.Length == 0) return null;

        Unit pull = bosses[Random.Range(0, bosses.Length)].Copy(team);

        return ScaleUnit(pull, true);
    }

    public Unit[] GetRandomArmy(int size)
    { 
        //Debug.LogWarning($"DEBUGGING : {name} Generator starting from {possibleLineUps.Length} possible units...");

        List<UnitGenerator> offers = new(possibleLineUps);
        List<Unit> picks = new(); 

        while(offers.Count > 0 && picks.Count < size)
        {
            int pull = Random.Range(0, offers.Count); 

            picks.Add(ScaleUnit(offers[pull].Copy(team))); 
            offers.RemoveAt(pull);
        }

        if (bosses.Length > 0 && picks.Count > 0)
            picks[0] = GetRandomBoss();

        return picks.ToArray();
    }

    // Creates reward heroes
    public Hero[] GetRandomHeroes(bool rare = false)
    {
        HeroClass[] picks = rare ? rewards.Get3RareRandomHeroes() : rewards.Get3RandomHeroes();
        List<Hero> list = new();
        foreach (var item in picks)
        {
            Debug.Log("Class loaded from generation list = " + (item ? item.name : "NULL"));
             
            Hero h = new(5, item);
            h.Initialize();
            h.Refresh(1, true);
            var equipment = h.equipment;
            equipment.weapon = h.heroClass.startingEquipment.weapon;
            equipment.armor = h.heroClass.startingEquipment.armor;
            equipment.accessoryA = h.heroClass.startingEquipment.accessoryA;
            equipment.accessoryB = h.heroClass.startingEquipment.accessoryB;
            h.plan.GenerateSkillFresh(h, false);
            list.Add(h);

            h.characterName = h.heroClass.GenerateRandomName();
        }

        return list.ToArray();
    }

    public Item[] GetRewards(bool rare = false)
    {
        return rare ? rewards.Get3RareRandomItems() : rewards.Get3RandomItems();
    }

    public CommandCard[] GetNewCards(bool rare = false)
    {
        return rare ? rewards.Get3RareRandomCommands() : rewards.Get3RandomCommands();
    }

    internal List<Item> GetStore()
    {
        return new List<Item>(rewards.Get10StoreItems());
    }



    public Unit ScaleUnit(Unit input, bool boss = false)
    {
        if (input == null || input.heroes == null || input.heroes.Count == 0)
            return input;

        int bossBoost = boss ? GameManager.Player.currentWorld.difficulty : 0;

        for (int i = 0; i < input.heroes.Count; i++)
        {
            Hero hero = input.heroes[i];

            if (i == input.leaderIndex && boss)
                hero.level = Mathf.Min(maxLevel + bossBoost, 20);

            else hero.level = Mathf.Clamp(Random.Range(minLevel, maxLevel) + bossBoost, 1, 20);
        }

        return input;
    }
}
