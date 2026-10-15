using UnityEngine; 

public class UI_QuickEquipMenu : MonoBehaviour
{
    static UI_QuickEquipMenu instance;

    public UI_ValidEquipHeroLoader loader;
    public UI_ShowCurrentHero currentHero;
    public UI_QuickEquipItemTooltip tooltip;
    public UI_InventoryLoader_LayoutGroup inventoryGroup;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        loader.LoadHeroes(InventoryManager.FindAllValidHeroes());
        currentHero.LoadHero(null);
        tooltip.PassObject(InventoryManager.picked.item);
    }

    public static void LoadPreview(Hero hero)
    {
        if (instance)
        {
            instance.currentHero.LoadHero(hero);

            instance.tooltip.PassPreviewIn(hero, InventoryManager.picked.item as Equipment);
        }
    }

    public static void Commit(Hero hero)
    {
        Equipment equip = InventoryManager.picked.item as Equipment; 
        if (hero == null || !instance || !equip) return;

        Item oldItem = null;

        if (equip is Weapon)
        {
            oldItem = hero.equipment.weapon;
            hero.equipment.weapon = equip as Weapon;
        }
        if(equip is Armor)
        {
            oldItem = hero.equipment.armor;
            hero.equipment.armor = equip as Armor;
        }
        else // Accessory
        {
            if (hero.equipment.accessoryA == null)
                hero.equipment.accessoryA = equip;

            else if (hero.equipment.accessoryB == null)
                hero.equipment.accessoryB = equip;

            else
            {
                Equipment accessoryA = hero.equipment.accessoryA;
                Equipment accessoryB = hero.equipment.accessoryB;

                if (accessoryA.rarity <= accessoryB.rarity)
                {
                    oldItem = accessoryA;
                    hero.equipment.accessoryA = equip;
                }
                else
                {
                    oldItem = accessoryB;
                    hero.equipment.accessoryB = equip;
                }
            }
        }

        GameManager.Inventory.AddItem(oldItem); // old item 
        GameManager.Inventory.RemoveItem(equip); // new item
        BannerArray.PrintReport($"{hero.characterName} has equipped {equip.itemName}.");

        ActiveMenu.CloseByID("Quick Equip");

        if (instance)
            instance.inventoryGroup.LoadInventory();
    }
}
