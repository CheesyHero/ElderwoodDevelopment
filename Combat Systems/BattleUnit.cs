using System.Collections.Generic;
using UnityEngine; 

public class BattleUnit : MonoBehaviour
{
    public float startingHP = 1;
    public float currentHP = 1;
    public float difference = 0;
    public float startingKo = 0;
    public float knockOuts = 0;
    public float workPoints = 0;

    [Header("Systems")]
    public static readonly List<BattleUnit> AllUnits = new();
    public List<BattleCombatant> children = new();
    public List<BattleCombatant> minions = new();

    public float actorOffset = 4f;
    public float actorRotation = 130f; 
    public bool Attacker = true;
    public BattleCombatant combatantPrefab;
    public Unit_PlaceMinions minionRack;
    public Unit parent;
    public SpriteRenderer banner;

    [SerializeField]
    BattleTile[][] tiles = new BattleTile[2][];

    public HexWalker GetMapParent()
    {
        if (Attacker)
            return HexWalker.battle_offense;

        else return HexWalker.battle_defense;

    }

    private void Awake()
    {
        AllUnits.Add(this);

        for (int i = 0; i < tiles.Length; i++)
            tiles[i] = new BattleTile[3];

        var hits = GetComponentsInChildren<BattleTile>(true);
        foreach (var find in hits)
        {
            Vector2 pos = find.GetUnitPosition();
            int x = Mathf.RoundToInt(pos.x);
            int y = Mathf.RoundToInt(pos.y);

            if (x < 0 || x >= tiles.Length || y < 0 || y >= tiles[x].Length)
            {
                //Debug.LogWarning($"{name} ignored BattleTile {find.name} at invalid index ({x},{y})");
                continue;
            }

            if (tiles[x][y] == null)
                tiles[x][y] = find;
        }
         
    }
    private void Update()
    {
        currentHP = parent.GetTotalHealthPercent();
        difference = startingHP - currentHP;
        knockOuts = parent.GetTotalKOs() - startingKo;
        workPoints = (difference * 500) + (knockOuts * 100);
    }
    private void OnDestroy()
    {
        AllUnits.Remove(this);
    }
    private void Start()
    {  
        transform.localPosition = new Vector3(Attacker ? -actorOffset : actorOffset, 0, 0);
        transform.localEulerAngles = new Vector3(0, Attacker ? actorRotation : -actorRotation, 0);
    }
    public void LoadUnits()
    {
        //Debug.Log($"Open: {name} is creating new units..."); 

        //Debug.Log($"Create: {name} creating combatant instances for {parent.heroes.Count} heroes...");
        for (int x = 0; x < parent.placements.Length; x++) 
            for(int y = 0; y < parent.placements[x].Length; y++)
            {
                //Debug.Log($"SPAWNING UNIT: {name} is checking tile index at ({x},{y})...");
                var i = CreateCombatant(parent.GetFromPlacement(x,y), GetTile(x, y));
                if (i != null)
                { children.Add(i); i.parent = this; }
            }

        startingHP = parent.GetTotalHealthPercent();
        startingKo = parent.GetTotalKOs();
         
        if (banner) banner.color = parent.team.teamColor;
    }
    protected BattleCombatant CreateCombatant(Hero hero, BattleTile tile)
    {
        if (hero != null && combatantPrefab != null && tile != null)
            return tile.SetHero(combatantPrefab, hero, parent.team, Attacker);

        return null;
    }

    public BattleTile GetTile(int x, int y)
    {
        if (x < 0 || x >= tiles.Length || y < 0 || y >= tiles[x].Length)
            return null;

        return tiles[x][y];
    }

    public void CreateCopy(BattleCombatant target, float copyStats = 0.5f)
    {
        Debug.Log($"Attempting to create a new hero : Copy of {target.name}");

        Hero copy = target.hero.Copy();

        Debug.Log($"Attempting to create a new hero : Copy of {copy.characterName}");
        BattleCombatant spawn = CreateMinion(copy);

        spawn.actionPoints = target.actionPoints;
        spawn.reactionPoints = target.reactionPoints;

        spawn.hero.health.health_current *= copyStats; 
        spawn.hero.health.health_max *= copyStats;
        spawn.hero.health.mana_current = 0;

        spawn.modifiers.power = copyStats;
        spawn.modifiers.armor = copyStats;
        spawn.modifiers.magic = copyStats;
        spawn.modifiers.resist = copyStats;

        spawn.transform.localScale *= copyStats;
        spawn.actionPoints = Mathf.CeilToInt((float)spawn.actionPoints / 2f);
        spawn.reactionPoints = Mathf.FloorToInt(((float)spawn.reactionPoints - 1) / 2f);

        minionRack.PlaceCombatantOnNextTile(spawn); 
    }
    public BattleCombatant CreateMinion(Hero minionHero)
    {
        if (minionHero == null || combatantPrefab == null) return null;
        if (minions.Count >= 4) return null;

        BattleCombatant minionActor = Instantiate(combatantPrefab);
        minionActor.hero = minionHero;
        minionActor.isMinion = true;
        minionActor.parent = this;
        minionActor.team = parent.team;
        minionActor.Initialize();
        minions.Add(minionActor);

        Debug.Log($"Created new minion : {minionActor.name}");

        return minionActor;
    }

    public bool IsAlive()
    {
        foreach(var item in parent.heroes)
            if(item.health.IsAlive) return true;

        return false;
    }

    public bool IsDefeated()
    {
        foreach(var item in parent.heroes)
            if(item.health.IsAlive) return false;

        return true;
    }
    public static int GetActivePoints()
    {
        int total = 0;
        foreach (var unit in AllUnits)
            total += unit.TotalAP();

        return total;
    }
    public int TotalAP()
    {
        int total = 0;
        foreach (var hero in children)
            total += hero.hero.health.IsAlive ? hero.actionPoints : 0;

        foreach (var minion in minions)
            total += minion.hero.health.IsAlive ? minion.actionPoints : 0;

        return total;
    }
    public void CleanUp()
    {
        foreach(var item in children) 
            Destroy(item.gameObject); 

        foreach(var item in minions)
            Destroy(item.gameObject);

        children.Clear();
        minions.Clear();

        minionRack.CleanAndHideTiles();
    }

    public void ApplyTeamEffects()
    { 
        foreach (var effect in parent.Effects)
        { 
            foreach (var hero in children)
            { 
                StatusCondition status = new()
                {
                    status = effect,
                    source = null,
                    target = hero,
                    stacks = 1,
                    duration = effect.duration
                };
                hero.GainStatus(null, status);
            }
        }

        parent.ClearAllEffects();
    }

    public bool HasFrontRow() => GetFirstRow().Count > 0;
    public bool HasBackRow() => GetBackRow().Count > 0;
    public List<BattleCombatant> GetFullRow()
    {
        var front = GetFirstRow();
        var back = GetBackRow();

        if (front.Count >= 3)
            return front;

        else if(back.Count >= 3)
            return back;

        return null;
    }  
    public List<BattleCombatant> GetLargestRow()
    {
        var front = GetFirstRow();
        var back = GetBackRow();

        if (front.Count >= back.Count)
            return front;

        else return back;
    } 
    public List<BattleCombatant> GetFirstRow()
    {
        List<BattleCombatant> hits = new();
        foreach(var hero in children)
            if(hero.IsFrontRow()) hits.Add(hero);

        return hits;
    }
    public List<BattleCombatant> GetBackRow()
    {
        List<BattleCombatant> hits = new();
        foreach (var hero in children)
            if (hero.IsBackRow()) hits.Add(hero);

        return hits;
    }

    public void Retreat()
    {
        for(int x = 0; x < tiles.Length; x++)
        {
            for (int y = 0; y < tiles[x].Length; y++)
            { 
                tiles[x][y].WalkOut(Attacker);
            }
        }
    }
    public static void EndStage()
    {
        if (AllUnits.Count != 2) return;

        bool winner = AutoBattleManager.winner == AllUnits[0].parent; 

        bool isBase = HexWalker.DisputedTerritory is TeamBase;
        bool defenderAlive = AllUnits[1].currentHP > 0;

        bool attackerRetreats = !winner || (isBase && defenderAlive);

        if (attackerRetreats)
            AllUnits[0].Retreat();
        else
            AllUnits[1].Retreat();
    }
}
