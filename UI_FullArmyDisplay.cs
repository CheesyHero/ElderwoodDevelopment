using System;
using UnityEngine;
using UnityEngine.Events;

public class UI_FullArmyDisplay : MonoBehaviour
{ 
    public static Hero CurrentHero { get; private set; }
    public static Unit CurrentUnit { get; private set; } 
    [SerializeField] 
    protected UI_UnitPreviewLoader[] units;

    public static UnityAction<Unit> OnUpdateUnit;
    public static UnityAction<Hero> OnUpdateHero; 

    private void Awake()
    {
        units = GetComponentsInChildren<UI_UnitPreviewLoader>();
        OnUpdateUnit += Reload;
        OnUpdateHero += Reload;
    }
    private void OnDestroy()
    {
        OnUpdateUnit = null;
        OnUpdateHero = null;
    }
    private void Start()
    {
        LoadPlayerArmy();
    }
    private void Reload(Hero _) => LoadPlayerArmy(); 
    private void Reload(Unit _) => LoadPlayerArmy(); 
    public void LoadPlayerArmy()
    {
        LoadArmy(GameManager.Player); 
    }
    public void LoadArmy(Army army)
    {
        HideAll();

        for(int i = 0; i < units.Length; i++)
        {
            if (i < army.units.Count)
                units[i].LoadUnit(army.units[i]);

            else units[i].LoadUnit(null);
        }
    }
    public void HideAll()
    {
        foreach(var item in units)
            item.SetActive(false);
    }
    public static void SetNewCurrentUnit(Unit next)
    { 
        //Debug.Log($"ARMY : Unit Select  : recieved hero = {next}");
        CurrentUnit = next;
        OnUpdateUnit?.Invoke(CurrentUnit);
    }
    public static void SetNewCurrentHero(Hero next)
    { 
        //Debug.Log($"ARMY : Hero Select : recieved hero = {(next != null ? next : "null")} " +
         //   $"at index ({UI_UnitSlot.LastIndex.x},{UI_UnitSlot.LastIndex.y})");
        CurrentHero = next;
        OnUpdateHero?.Invoke(CurrentHero);
    }
    public static void ExchangeHero(Hero next)
    {
        int x = (int)UI_UnitSlot.LastIndex.x;
        int y = (int)UI_UnitSlot.LastIndex.y;

        //Debug.Log($"Attempting to update hero to: {(next != null ? next : "null")} @ index ({x},{y})");
         
        Hero pull = CurrentUnit.GetFromPlacement(x, y);
         
        if (next != null && pull != null && next == pull)
        {
            BannerArray.PrintReport($"{next} has been removed from ({x},{y})");
            CurrentUnit.ClearIndex(x, y, true); 
             
            if (CurrentHero != null && CurrentHero == next)
                SetNewCurrentHero(null);

            SetNewCurrentUnit(CurrentUnit);
            return;
        }
         
        if (next != null && CurrentHero != null && next == CurrentHero)
        {
            (int hx, int hy) = CurrentUnit.GetPlacementOf(CurrentHero);

            if (hx >= 0 && hy >= 0)
            {
                BannerArray.PrintReport($"{CurrentHero} has been removed from ({hx},{hy})");
                CurrentUnit.ClearIndex(hx, hy, true);
            }

            SetNewCurrentHero(null);
            SetNewCurrentUnit(CurrentUnit);
            return;
        }
         
        if (next != null && CurrentUnit != null && CurrentUnit.AtCapacity)
        {
            (int nx, int ny) = CurrentUnit.GetPlacementOf(next);
            bool nextAlreadyInCurrentUnit = (nx >= 0 && ny >= 0);
            bool isReplacingFilledSlot = (pull != null);

            if (!isReplacingFilledSlot && !nextAlreadyInCurrentUnit)
            {
               BannerArray.PrintReport(
                    $"Cannot add {next} to CurrentUnit: unit is at max capacity ({CurrentUnit.maxPlacements})."
                );
                return;
            }
        }
         
        Unit exists = GameManager.Player.FindExistingPlacedUnitOfHero(next);

        if (exists != null)
            OtherDoesExist(exists, pull, next, x, y);
        else
            OtherDoesNotExist(pull, next, x, y);

        SetNewCurrentHero(next);
        SetNewCurrentUnit(CurrentUnit);
    }


    protected static void OtherDoesNotExist(Hero pull, Hero next, int x, int y)
    {
        if (pull != null)
            RemoveAndPlace(next, x, y);
        else
            PlaceHeroInEmpty(next, x, y);
    }
    protected static void PlaceHeroInEmpty(Hero next, int x, int y)
    {
        if (x <= -1 || y <= -1) return;

        Debug.Log($"No existing hero. Placing unit into ({x},{y})");
        CurrentUnit.PlaceHero(next, x, y);
    }
    protected static void RemoveAndPlace(Hero next, int x, int y)
    {
        if (x <= -1 || y <= -1) return;

        Debug.Log($"Hero is not currently placed. Exchanging.");
        CurrentUnit.ClearIndex(x, y, true);
        CurrentUnit.PlaceHero(next, x, y);
    }
    protected static void OtherDoesExist(Unit exists, Hero pull, Hero next, int x, int y)
    {
        if (pull != null) // another hero is in our desired placement
            ExchangeWithOtherUnit(exists, pull, next);
        else
            RemoveFromOtherAndPlace(exists, next, x, y);
    }
    protected static void RemoveFromOtherAndPlace(Unit exists, Hero next, int x, int y)
    {
        exists.RemoveHero(next);
        CurrentUnit.PlaceHero(next, x, y);
    }
    protected static void ExchangeWithOtherUnit(Unit exists, Hero pull, Hero next)
    {
        Debug.Log($"Hero exists in another unit: {exists} @ {exists.GetPlacementOf(next)}");
        CurrentUnit.ExchangeWithOtherUnit(pull, next, exists);
    }
    public static void ChangeEquipment(Enum_EquipmentSlot slot, Equipment next)
    {
        Debug.Log($"Attempting to change equipment of slot: {slot}");
        if (CurrentHero.CanUseEquipment(next) == false)
        { Debug.Log($"{CurrentHero} cannot equip {next} : type mismatch");  return; }

        switch (slot)
        {
            default: return;
            case Enum_EquipmentSlot.weapon:
                CurrentHero.equipment.ChangeWeapon(next as Weapon, GameManager.Player.inventory);
                break;
            case Enum_EquipmentSlot.armor:
                CurrentHero.equipment.ChangeArmor(next as Armor, GameManager.Player.inventory);
                break;
            case Enum_EquipmentSlot.accessoryA:
                CurrentHero.equipment.ChangeAccessoryA(next, GameManager.Player.inventory);
                break;
            case Enum_EquipmentSlot.accessoryB:
                CurrentHero.equipment.ChangeAccessoryB(next, GameManager.Player.inventory);
                break;
        }

        CurrentHero.plan.ValidateMoves(CurrentHero);

        //CurrentHero.plan.ReloadSkills(CurrentHero);
        OnUpdateHero?.Invoke(CurrentHero);
    }

    public static void ChangeName(string newName)
    {
        if (CurrentHero == null) return;

        CurrentHero.characterName = newName;
        OnUpdateHero?.Invoke(CurrentHero);
        OnUpdateUnit?.Invoke(CurrentUnit);
    }
     
    public static Action SetUpMedalButton;
    public static void SetConfirmUnitUpgrade()
    { 
        SetUpMedalButton = UpgradeCurrentUnit;
    }
    public static void SetConfirmHeroPromotion()
    { 
        SetUpMedalButton = PromoteCurrentHero;
    }
    public static void SetConfirmAddUnit()
    {
        SetUpMedalButton = AddNewUnitToPlayerArmy; 
    }
    public static void DoSelectedNext()
    {
        SetUpMedalButton?.Invoke();
    } 
    public static void UpgradeCurrentUnit()
    {
        if (CurrentUnit == null)
            return;

        int cost = BattleConstants.GetUnitSizeUpgradeCost(CurrentUnit);

        if (cost < 0)
            return;

        if (cost > 0 && GameManager.Inventory.CanSpend(cost) == false)
            return;

        if (cost > 0)
            GameManager.Inventory.SpendMoney(cost);

        CurrentUnit.maxPlacements++;

        OnUpdateUnit?.Invoke(CurrentUnit);
    }

    public static void PromoteCurrentHero()
    {
        if (CurrentHero == null)
            return;

        if (CurrentHero.heroClass.HasPromotionRequirement())
            CurrentHero.Promote();

        OnUpdateHero?.Invoke(CurrentHero);
    }

    public static void AddNewUnitToPlayerArmy()
    {
        int cost = BattleConstants.GetAddUnitCost(GameManager.Player);

        if (cost < 0)
            return;

        if (cost > 0 && GameManager.Inventory.CanSpend(cost) == false)
            return;

        if (cost > 0)
            GameManager.Inventory.SpendMoney(cost);

        Unit unit = new()
        { team = GameManager.Player.team };
        GameManager.Player.units.Add(unit);

        OnUpdateHero?.Invoke(CurrentHero);
        OnUpdateUnit?.Invoke(CurrentUnit);
    }


    // // // Change Move Buttons // // //

    public static void ChangeMove(BattleMove move, int index)
    {
        if (CurrentHero == null || move == null) return;
        Debug.Log($"CHANGE MOVE: {move} : index: {index} = Onto hero {CurrentHero}");

        CurrentHero.plan.ChangeMove(move, index);
        SetNewCurrentHero(CurrentHero);
    }

    public static void Reload()
    {
        OnUpdateHero?.Invoke(CurrentHero);
        OnUpdateUnit?.Invoke(CurrentUnit);
    }
}

