using System.Collections.Generic; 
using UnityEngine;

[System.Serializable]
public class Unit
{
    public string note;
    [Range(1, 5)]
    public int maxPlacements = 2;
    public int leaderIndex = 0;
    public Team team = new();
    public List<Hero> heroes = new();
    public Vector3[] debugTestSettings = new Vector3[] 
    { new(-1, -1, -1), new(-1, -1, -1)};

    public bool AtCapacity { get { return heroes.Count >= maxPlacements; } }
    public int[][] placements = new int[2][];
    private int cooldown = -1;
    public int Cooldown { get { return cooldown; } }
    [SerializeField] private List<Passive> teamEffects = new();
    public Passive[] Effects { get { return teamEffects.ToArray(); } } 
    public void ClearAllEffects() => teamEffects.Clear();
    public void AddEffect(Passive condition)
    {
        if(teamEffects.Contains(condition)) return;
        teamEffects.Add(condition);
    }


    public Unit()
    {
        placements = new int[2][];
        for (int i = 0; i < placements.Length; i++)
        { placements[i] = new int[] { -1, -1, -1 }; }
    }
    public Hero Leader()
    {
        if (leaderIndex >= heroes.Count || leaderIndex < 0)
            leaderIndex = 0;

        if (leaderIndex < heroes.Count)
            return heroes[leaderIndex];

        else return null;
    }
    public void SetAsLeasder(Hero hero)
    {
        Debug.Log($"Attempting to place new leader : {hero}");

        if (heroes.Contains(hero))
            leaderIndex = IndexOf(hero);  
    }
    public int IndexOf(Hero hero)
    {
        if(heroes.Contains(hero))
            return heroes.IndexOf(hero);

        else return -1;
    }
    public int GetMoves()
    {
        if (leaderIndex < heroes.Count)
            return heroes[leaderIndex].heroClass.leaderTraits.maxMoves;

        else return 3;
    }
    public override string ToString()
    {
        if (heroes == null || heroes.Count == 0)
            return base.ToString();

        if (leaderIndex < 0 || leaderIndex >= heroes.Count || heroes[leaderIndex] == null)
            return $"{team} : No Leader";

        return $"{team} : {heroes[leaderIndex].characterName}";
    }
    public void Refresh(float percent, bool mana)
    {
        cooldown = 0;
        foreach (var hero in heroes)
            hero.Refresh(percent, mana);
    }
    public void Initialize(bool loadDebug)
    {
        if (maxPlacements < heroes.Count)
            maxPlacements = heroes.Count;

        foreach (var item in heroes)
        {
            item.team = team;
            item.Initialize();
        }

        if (loadDebug)
            LoadDebugTest();
        else
            UpdateDebugTestSettingsFromPlacements();
    }

    public void AddHero(Hero hero)
    {
        Debug.Log($"Attempting to ADD hero {hero}");
        if (heroes.Count >= maxPlacements)
            Debug.LogWarning($"{this} cannot have more than {maxPlacements} placements");

        else if (hero != null && heroes.Contains(hero) == false)
            heroes.Add(hero);
    }
    public void RemoveHero(Hero hero)
    {
        Debug.Log($"Attempting to REMOVE hero {hero.characterName}");
        if (hero == null) return;

        int removedIndex = heroes.IndexOf(hero);
        if (removedIndex < 0) return;

        // Clear any placements pointing at this hero
        for (int x = 0; x < placements.Length; x++)
            for (int y = 0; y < placements[x].Length; y++)
                if (placements[x][y] == removedIndex)
                    placements[x][y] = -1;

        // Remove from list
        heroes.RemoveAt(removedIndex);

        // Reindex: every placement index greater than removedIndex must decrement
        for (int x = 0; x < placements.Length; x++)
            for (int y = 0; y < placements[x].Length; y++)
                if (placements[x][y] > removedIndex)
                    placements[x][y]--;
         
        UpdateDebugTestSettingsFromPlacements();
    }
    private void CorrectLeaderIndex()
    {
        if(leaderIndex >= heroes.Count)
            leaderIndex = 0;
    }

    public Hero GetFromPlacement(int x, int y)
    {
        //Debug.Log($"Searching unit for existing unit at ({x},{y}) with result result {placements[x][y]}");

        if (placements[x][y] < heroes.Count)
            return placements[x][y] >= 0 ? heroes[placements[x][y]] : null;
        else return null;
    }
    public Hero ExtractHeroFromIndex(int x, int y, bool clear)
    {
        try
        { 
            Hero extract = heroes[placements[x][y]]; 
            ClearIndex(x, y, clear);
            return extract;
        }
        catch
        { return null; }
    } 
    public void SwapIndexes(int x1, int y1, int x2, int y2)
    {
        try
        {
            (placements[x2][y2], placements[x1][y1]) = (placements[x1][y1], placements[x2][y2]);
            UpdateDebugTestSettingsFromPlacements();
        }
        catch
        { return; }
    }
    public void ClearIndex(int x, int y, bool removeHero)
    {
        ClearIndex(x, y, removeHero, true);
    }

    private void ClearIndex(int x, int y, bool removeHero, bool updateDebug)
    {
        try
        {
            if (removeHero)
                RemoveHero(GetFromPlacement(x, y));

            placements[x][y] = -1;

            if (updateDebug)
                UpdateDebugTestSettingsFromPlacements();
        }
        catch
        { return; }
    }
    public void PlaceHero(Hero hero, int x, int y)
    {
        AddHero(hero);

        int heroIndex = heroes.Contains(hero) ? heroes.IndexOf(hero) : -1;
        PlaceHero(heroIndex, x, y);
    }
    public void PlaceHero(int heroIndex, int x, int y)
    {
        PlaceHero(heroIndex, x, y, true);
    }

    private void PlaceHero(int heroIndex, int x, int y, bool updateDebug)
    {
        if (IndexIsPlaced(heroIndex))
        {
            (int row, int col) = GetPlacementOf(heroIndex);
            ClearIndex(row, col, false, false);
        }

        if (heroIndex >= -1 && heroIndex < heroes.Count)
        {
            try { placements[x][y] = heroIndex; }
            catch { return; }
        }
        else if (heroIndex == -1)
        {
            placements[x][y] = -1;
        }

        if (updateDebug)
            UpdateDebugTestSettingsFromPlacements();
    }
    public bool IndexIsPlaced(int heroIndex)
    {
        for (int x = 0; x <  placements.Length; x++) 
            for(int y = 0; y < placements[x].Length; y++)
            {
                //Debug.Log($"UNIT PLACEMENT: Desired hero index of {heroIndex} is checking placement ({x},{y})");
                if (placements[x][y] == heroIndex)
                    return true;
            }

        return false;
    }
    public (int, int) GetPlacementOf(Hero check)
    {
        if (heroes.Contains(check))
            return GetPlacementOf(heroes.IndexOf(check));

        else return (-99, -99);
    }
    public (int, int) GetPlacementOf(int heroIndex)
    {
        for (int x = 0; x < placements.Length; x++)
            for (int y = 0; y < placements[x].Length; y++)
                if (placements[x][y] == heroIndex)
                    return new (x, y);

        return (-99, -99);
    }
    public void ExchangeWithOtherUnit(Hero ourHero, Hero otherPull, Unit otherUnit)
    {
        (int x, int y) ourPos = GetPlacementOf(heroes.IndexOf(ourHero));
        (int x, int y) otherPos = otherUnit.GetPlacementOf(otherUnit.heroes.IndexOf(otherPull));

        Debug.Log($"Attempting to swap units between {this} @ ({ourHero}) and {otherUnit} @ ({otherPull})");
        ExchangeWithOtherUnit(ourPos.x, ourPos.y, otherUnit, otherPos.x, otherPos.y); 
    }
    public void ExchangeWithOtherUnit(int x1, int y1, Unit otherUnit, int x2, int y2)
    {
        Debug.Log($"Attempting to swap units between {this} @ ({x1},{y1}) and {otherUnit} @ ({x2},{y2})");

        int o = leaderIndex;
        bool sameUnit = otherUnit == this;

        Hero ourHero = ExtractHeroFromIndex(x1, y1, sameUnit);
        Hero otherPull = otherUnit.ExtractHeroFromIndex(x2, y2, sameUnit);

        PlaceHero(otherPull, x1, y1);
        otherUnit.PlaceHero(ourHero, x2, y2);

        if (sameUnit) leaderIndex = 0;

        UpdateDebugTestSettingsFromPlacements();
    }
    public void LoadDebugTest()
    {
        if (debugTestSettings.Length != 2) return;

        for (int x = 0; x < debugTestSettings.Length; x++)
        {
            int[] column = GetVector3AsArray(debugTestSettings[x]);

            List<int> tileIndexes = new();
            for (int y = 0; y < column.Length; y++)
                tileIndexes.Add(y);

            Shuffle(tileIndexes);

            for (int i = 0; i < column.Length; i++)
            {
                int heroIndex = column[i];
                int randomY = tileIndexes[i];

                PlaceHero(heroIndex, x, randomY, false);
            }
        }

        UpdateDebugTestSettingsFromPlacements();
    }
    public void UpdateDebugTestSettingsFromPlacements()
    {
        if (placements == null || placements.Length != 2)
            return;

        CorrectLeaderIndex();

        if (debugTestSettings == null || debugTestSettings.Length != placements.Length)
            debugTestSettings = new Vector3[placements.Length];

        for (int x = 0; x < placements.Length; x++)
            debugTestSettings[x] = GetArrayAsVector3(placements[x]);
    }
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }


    public void ViewDebugTestPlacements()
    {
        if (debugTestSettings.Length != 2) return;

        Debug.Log($"Unit with {heroes.Count} heroes is setting debug placements...");
        for (int x = 0; x < placements.Length; x++) // horizontal
            for (int y = 0; y < placements[x].Length; y++) // vertical 
                debugTestSettings[x] = GetArrayAsVector3(placements[x]); 
    }
    public static int[] GetVector3AsArray(Vector3 input)
    {
        return new int[] { (int)input.x, (int)input.y, (int)input.z };
    }
    public static Vector3 GetArrayAsVector3(int[] input)
    {
        return new Vector3(input[0], input[1], input[2]);
    }

    public bool IsAlive()
    {
        if (cooldown > 0) return false;

        for (int x = 0; x < placements.Length; x++)
            for (int y = 0; y < placements[x].Length; y++)
            {
                int idx = placements[x][y];
                if (idx >= 0 && idx < heroes.Count)
                    if (heroes[idx].health.IsAlive)
                        return true;
            }

        return false;
    } 
    public bool IsDefeated()
    {
        if (cooldown > 0) return true;

        for (int x = 0; x < placements.Length; x++)
            for (int y = 0; y < placements[x].Length; y++)
            {
                int idx = placements[x][y];
                if (idx >= 0 && idx < heroes.Count)
                    if (heroes[idx].health.IsAlive)
                        return false;
            }

        return true;
    }
    public float GetTotalHealthPercent()
    {
        if (heroes.Count <= 0) return 0;

        float current = 0, max = 0;
        foreach(var hero in heroes)
        {
            current += hero.health.health_current;
            max += hero.health.health_max;
        }

        return current / max;
    }
    public void TickCooldown(int count = 1)
    {
        if (cooldown <= 0) return;

        cooldown -= count;
        if(cooldown <= 0) 
            Refresh(1, true); 
    }
    public void StartCooldown(int time = 3)
    {
        cooldown = time;
    }

    public int GetTotalEXP()
    {
        int expTotal = 0;
        foreach (var item in heroes)
            expTotal += item.GetReward();

        return expTotal;
    }

    public void GrantEXP(float exp)
    { 
        foreach (var item in heroes)
            item.GainExperience(Mathf.CeilToInt(exp)); 
    }
    public Unit Copy()
    {
        Unit copy = new()
        {
            maxPlacements = this.maxPlacements,
            leaderIndex = this.leaderIndex,
            team = this.team, 
            cooldown = this.cooldown
        };

        foreach (var hero in heroes)
            copy.heroes.Add(hero.Copy());

        copy.placements = new int[placements.Length][];
        for (int x = 0; x < placements.Length; x++)
        {
            copy.placements[x] = new int[placements[x].Length]; 
            for (int i = 0; i < placements[x].Length; i++) 
                copy.placements[x][i] = placements[x][i]; 
        }

        copy.debugTestSettings = (Vector3[])debugTestSettings.Clone();

        return copy;
    }

    public float GetTotalKOs()
    {
        int total = 0;
        foreach(var item in heroes)
            if(item.health.IsDead)
                total++;

        return total;
    }

    public void RefreshCooldown()
    {
        cooldown = 0;
        Refresh(100, true);
    }
}
[System.Serializable]
public class Row_Unit
{
    public Hero[] collumn = new Hero[3];
}
