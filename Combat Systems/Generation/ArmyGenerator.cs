using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;

[CreateAssetMenu(fileName = "New Army", menuName = "NMD/Encounters/Army Generator")]
public class ArmyGenerator : ScriptableObject
{
    public GameRewards rewards;
    public Team team;
    public Team mirror;
    public UnitGenerator[] possibleLineUps;
    public UnitGenerator[] bosses;

    [Header("Command Card Deck")]
    public CommandCard[] commands;

    public Team GetTeam(bool _ = false) => team;
    public Unit GetRandomUnit()
    {
        if (possibleLineUps == null || possibleLineUps.Length == 0) return null;

        Unit pull = possibleLineUps[Random.Range(0, possibleLineUps.Length)].Copy(team);

        return pull;
    }
    public Unit GetRandomBoss()
    {
        if (bosses == null || bosses.Length == 0) return null;

        Unit pull = bosses[Random.Range(0, bosses.Length)].Copy(team);

        return pull;
    }

    public Unit[] GetRandomArmy(int size)
    {
        List<Unit> offers = new();
        List<Unit> picks = new();

        foreach(var item in possibleLineUps)
            offers.Add(item.Copy(team));

        while(offers.Count > 0 && picks.Count < size)
        {
            int pull = Random.Range(0, offers.Count);
            picks.Add(offers[pull]);
            offers.RemoveAt(pull);
        }

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
            h.plan.GenerateSkillFresh(h);
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
}
