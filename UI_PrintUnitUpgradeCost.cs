using UnityEngine;
using UnityEngine.UI;

public class UI_PrintUnitUpgradeCost : ComponentLoader
{
    public Button button;

    public void LoadCurrentHero() => PassObject(UI_FullArmyDisplay.CurrentHero);
    public void LoadCurrentUnit() => PassObject(UI_FullArmyDisplay.CurrentUnit);
    public void LoadCurrentArmy() => PassObject(GameManager.Player);

    public override void PassObject(object obj)
    {
        if (obj == null)
            return;

        int ownedMoney = GameManager.Inventory.GetMoney();

        if (obj is Unit unit)
        {
            int cost = BattleConstants.GetUnitSizeUpgradeCost(unit);

            if (cost < 0)
            {
                SetText(GetUnitMaxPrint(unit.maxPlacements));
                if (button) button.interactable = false;
                return;
            }

            SetText(GetUnitPrint(cost, ownedMoney, unit.maxPlacements));
            if (button) button.interactable = GameManager.Inventory.CanSpend(cost);
        }
        else if (obj is Hero hero)
        {
            int cost = BattleConstants.GetHeroPromotionCost(hero);

            if (cost < 0)
            {
                SetText(GetHeroMaxPrint(hero.heroClass.rank));
                if (button) button.interactable = false;
                return;
            }

            SetText(GetHeroPrint(cost, ownedMoney, hero.heroClass.rank));
            if (button) button.interactable = GameManager.Inventory.CanSpend(cost);
        }
        else if (obj is Army army)
        {
            int unitCount = army.units.Count;
            int cost = BattleConstants.GetAddUnitCost(army);

            if (cost < 0)
            {
                SetText(GetArmyMaxPrint(unitCount));
                if (button) button.interactable = false;
                return;
            }

            SetText(GetArmyPrint(cost, ownedMoney, unitCount));
            if (button) button.interactable = GameManager.Inventory.CanSpend(cost);
        }
    } 

    public string GetUnitPrint(int cost, int ownedMoney, int currentSize)
    {
        return $"Are you sure you want to spend <b>{cost}</b> <color=#74e8de>gold</color> to upgrade this unit's size?\n\n" + 
               $"Unit size {currentSize} -> <b>{currentSize + 1}</b>";
    }

    public string GetHeroPrint(int cost, int ownedMoney, int currentRank)
    {
        return $"Are you sure you want to spend <b>{cost}</b> <color=#74e8de>gold</color> to promote this hero?\n\n" + 
               $"Hero rank {currentRank} -> <b>{currentRank + 1}</b>";
    }

    public string GetArmyPrint(int cost, int ownedMoney, int currentUnitCount)
    {
        return $"Are you sure you want to spend <b>{cost}</b> <color=#74e8de>gold</color> to recruit a new unit?\n\n" + 
               $"Army units {currentUnitCount} -> <b>{currentUnitCount + 1}</b>";
    }

    public string GetUnitMaxPrint(int currentSize)
    {
        return $"This unit is already at max size.\n" +
               $"<i>Unit size: {currentSize}</i>";
    }

    public string GetHeroMaxPrint(int currentRank)
    {
        return $"This hero is already at max rank.\n" +
               $"<i>Hero rank: {currentRank}</i>";
    }

    public string GetArmyMaxPrint(int currentUnitCount)
    {
        return $"This army is already at the max number of units.\n" +
               $"<i>Army units: {currentUnitCount}</i>";
    }
}