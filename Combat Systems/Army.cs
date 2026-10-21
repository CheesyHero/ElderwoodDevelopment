using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class Army : MonoBehaviour
{
    public bool defeated = false;

    [Header("Random Generation")]  
    public int firstTimeArmySize = 4;
    public ArmyGenerator generationList; 

    [Header("Card Deck")]
    [Range(0, 10)]
    public int commandPoints = 0;
    public DeckManager deck;

    [Header("Game Systems")]
    public TeamBase homeBase;
    public string playerID = "AI";
    public Team team = new();
    public List<Hero> party = new();
    public List<Unit> units = new();
    public Inventory inventory = new();
    public int knockOutsThisGame = 0;

    public static UnityAction<Army> OnCommandPointsGained;
    public static UnityAction<Army> OnCommandPointsLost;

    public void KnockOut(Unit unit)
    {
        if (units.Contains(unit))
        {
            knockOutsThisGame++;
            unit.StartCooldown(knockOutsThisGame);
        }
    }
    public void StartTurn()
    {
        if (defeated) return;

        foreach (var item in units)
            item.TickCooldown(playerID == "AI" ? GameManager.CurrentLevel + 1 : 1);

        GainCommandPoints(1);
        DrawOnTurnStart();
    }
    public void DrawOnTurnStart()
    {
        if (!deck) return;

        deck.DrawCard(TurnManager.Round == 0 ? 3 : 1); 
    }

    public void GenerateArmy(ArmyGenerator armyGen)
    {
        generationList = armyGen;
        GenerateArmy();
    }

    public void GenerateArmy()
    {
        //Debug.LogWarning($"DEBUGGING : {name} is generating an army...");

        if (generationList == null && playerID == "AI") generationList = GameManager.Player.lastNode.main; 
        if (generationList == null) return; // 

        int size = firstTimeArmySize; 
        if (playerID == "AI") size += GameManager.CurrentLevel * 2;

        //Debug.LogWarning($"DEBUGGING : {name} is attempting to load from {generationList}...");
        units = new(generationList.GetRandomArmy(size));
        team = generationList.GetTeam();

        if (playerID == "AI" && GameManager.CurrentEncounter.depth <= 2)
            return; // We don't want enemies early in runs to have strong bosses 
    }
    protected virtual void Awake()
    {
        if (units.Count == 0)
            GenerateArmy();

        PopulateArmy();

        deck = GetComponentInChildren<DeckManager>();
        if (deck) deck.LoadDeck(this);
    }
    public void PopulateArmy()
    {
        for (int i = 0; i < party.Count; i++)
        {
            Hero hero = party[i];
            hero.team = team;
            hero.Initialize();
            hero.Refresh(1, true);
        }
        for (int i = 0; i < units.Count; i++)
        {
            Unit unit = units[i];
            foreach (var hero in unit.heroes)
                if (!party.Contains(hero))
                    party.Add(hero);

            if (generationList && unit.heroes.Count == 0)
                unit = PullRandomGeneration();

            unit.team = team;
            unit.Initialize(true);
            unit.Refresh(1, true);
        }
    }
    public void Add(Hero hero)
    {
        if (hero == null)
            return;

        hero.team = team;
        hero.Initialize();
        hero.Refresh(1, true);
        party.Add(hero);
    }
    public virtual Unit PullRandomGeneration()
    {
        if (!generationList) return new();
        return generationList.GetRandomUnit();
    }
    public virtual Unit[] PullRandomGeneration(int amount)
    {
        if (amount <= 0 || !generationList)
            return new Unit[0];

        return generationList.GetRandomArmy(amount);
    }

    public int HighestLevel(float scale)
    {
        if (party == null || party.Count == 0)
            return 0;

        int highest = 0;
        foreach (var item in party)
        {
            if (item != null && item.level > highest)
                highest = item.level;
        }

        return Mathf.FloorToInt(highest * scale);
    }

    public int AverageLevel(float scale)
    {
        float total = 0;
        foreach (var item in party)
            total += item.level;

        return Mathf.FloorToInt(total / party.Count * scale);
    }
    public void AddHeroToUnit(int partyIndex, int unitIndex, int x, int y)
    {
        partyIndex = Mathf.Clamp(partyIndex, 0, party.Count);
        Hero hero = party[partyIndex];

        unitIndex = Mathf.Clamp(unitIndex, 0, party.Count);
        Unit unit = units[unitIndex];

        unit.PlaceHero(hero, x, y);
    }
    public void SetHomeBase(TeamBase stronghold)
    {
        homeBase = stronghold;
    }

    public Unit FindExistingPlacedUnitOfHero(Hero next)
    {
        foreach (var unit in units)
        {
            if (unit.heroes.Contains(next))
                return unit;
        }

        return null;
    }

    internal Vector3 GetTurnPoint()
    {
        foreach (var unit in units)
            if (HexWalker.Exists(unit))
                return HexWalker.Exists(unit).transform.position;
         
        return homeBase ? homeBase.transform.position : Vector3.zero;
    }

    public bool IsUnitLeader(Hero hero)
    {
        if (party.Contains(hero) == false) return false;

        foreach (var u in units)
            if (u.Leader() == hero)
                return true;

        return false;
    }
    public bool HasReserveArmyReady()
    {
        foreach (var u in units)
            if (HexWalker.Exists(u) == false && u.IsAlive())
                return true;

        return false;
    }
     
    public int GetRandomAvailableUnit()
    {
        List<Unit> valid = new();
        foreach(var u in units)
            if(!HexWalker.Exists(u) && u.IsAlive()) valid.Add(u);

        if (valid.Count == 0)
            return -1;

        return units.IndexOf(valid[Random.Range(0, valid.Count)]);
    }

    public HexTile GetRandomBase()
    {
        List<TeamBase> ownedBases = new();
        foreach (var item in TeamBase.bases)
            if (item.owner == team)
                ownedBases.Add(item);

        if(ownedBases.Count == 0) return null;
        return ownedBases[Random.Range(0, ownedBases.Count)];
    }
    public HexTile FindAvailableTile(Unit u)
    {
        if (!homeBase || homeBase.owner != team) return null;

        if (homeBase.CurrentUnit == null)
            return homeBase; // if there is no unit defending the base, place one

        // pick a random base we own
        HexTile selectTile = GetRandomBase();

        List<HexTile> hits = new(); 
        foreach (var adj in selectTile.adjacents)
            if (adj.CurrentUnit == null)
                hits.Add(adj);

        hits = FilterHexesByRoad(hits);

        if (hits.Count == 0) return null;

        if (hits.Count > 1)
        {
            for(int i = hits.Count - 1; i >= 0; i--)
            {
                if (u.Leader().heroClass.leaderTraits.ContainsUnfavoredType(hits[i].type))
                    hits.RemoveAt(i);
            }
        }

        return GetClosestToEnemy(hits);
    }
    public static List<HexTile> FilterHexesByRoad(List<HexTile> input)
    {
        var roadHits = input.FindAll(t => t != null && t.type == Enum_HexTile_Type.road);
        if (roadHits.Count > 0)
            return roadHits;

        else return input;
    }

    public HexTile GetClosestToEnemy(List<HexTile> list)
    {
        Vector3 closestEnemy = ClosestEnemyToBase();

        HexTile closest = null;
        float distance = 9999f;

        foreach(var item in list)
        {
            float d = Vector3.Distance(item.transform.position, closestEnemy);
            if (d < distance)
            {
                closest = item;
                distance = d;
            }
        }

        return closest;
    }
    public Vector3 ClosestEnemyToBase()
    {
        Vector3 point = Vector3.zero;
        float distance = 9999f;
        foreach (var walker in HexWalker.activeWalkers)
        {
            if (walker && walker.Team != team)
            {
                float d = Vector3.Distance(walker.transform.position, homeBase.transform.position);
                if (d < distance)
                {
                    point = walker.transform.position;
                    distance = d;
                }
            }
        }

        return point;
    }

    public void GainCommandPoints(int gain = 1)
    {
        gain = Mathf.Abs(gain);
        commandPoints = Mathf.Min(commandPoints + gain, 10);

        OnCommandPointsGained?.Invoke(this);
    }
    public void UseCommandPoints(int use = 1)
    {
        use = Mathf.Abs(use);
        commandPoints = Mathf.Max(commandPoints - use, 0);

        OnCommandPointsLost?.Invoke(this);
    }
    public void RefreshAll()
    {
        foreach (var u in units) 
            u.RefreshCooldown(); 

        commandPoints = 3;

        if(deck) deck.ResetZones();
    }

    // Call this whenever you want party ordering updated (after placements, before UI draw, etc.)
    public void SortPartyByUnits()
    {
        Unit focusUnit = UI_FullArmyDisplay.CurrentUnit;

        party ??= new List<Hero>();
        units ??= new List<Unit>();

        List<Hero> originalParty = new(party.Where(h => h != null));

        // Track heroes that are placed in ANY unit (including focusUnit)
        HashSet<Hero> heroesInUnits = new();
        foreach (var u in units)
        {
            if (u == null || u.heroes == null) continue;
            foreach (var h in u.heroes)
                if (h != null) heroesInUnits.Add(h);
        }

        // Build the focus chunk (if any)
        List<Hero> focusChunk = BuildUnitChunk(focusUnit);

        // Build all other unit chunks in order
        List<Hero> othersChunk = new();
        foreach (var u in units)
        {
            if (u == null) continue;
            if (focusUnit != null && ReferenceEquals(u, focusUnit)) continue;

            othersChunk.AddRange(BuildUnitChunk(u));
        }

        // Assemble: focus at top, then others, then reserves
        List<Hero> sorted = new();
        sorted.AddRange(focusChunk);
        sorted.AddRange(othersChunk);

        var reserves = originalParty
            .Where(h => h != null && !heroesInUnits.Contains(h))
            .Distinct()
            .OrderByDescending(GetHeroLevelSafe)
            .ToList();

        sorted.AddRange(reserves);

        party = DedupPreserveOrder(sorted);
    }

    private static List<Hero> BuildUnitChunk(Unit u)
    {
        if (u == null || u.heroes == null || u.heroes.Count == 0)
            return new List<Hero>();

        List<Hero> chunk = new();

        Hero leader = u.Leader();
        if (leader != null)
            chunk.Add(leader);

        var others = u.heroes
            .Where(h => h != null && h != leader)
            .OrderByDescending(GetHeroLevelSafe)
            .ToList();

        chunk.AddRange(others);
        return chunk;
    }
     
    private static int GetHeroLevelSafe(Hero hero) => hero == null ? -1 : hero.level; 

    private static List<Hero> DedupPreserveOrder(List<Hero> input)
    {
        List<Hero> result = new(input.Count);
        HashSet<Hero> seen = new();
        foreach (var h in input)
        {
            if (h == null) continue;
            if (seen.Add(h)) result.Add(h);
        }
        return result;
    }
    public int SpawnCost(int index)
    {
        if (index < 0 || index > units.Count)
            return -1;

        else return units[index].heroes.Count;
    }

    public void DrawCards(int v)
    {
        if (deck) deck.DrawCard(v);
    }
}
