using UnityEngine;

public class UI_GoldDisplay : ComponentLoader
{
    public string suffix = " Gold";
    public float countSpeed = 100f;

    [SerializeField] protected float internalValue = 0f;
    [SerializeField] protected float targetValue = 0f; 

    private void OnEnable()
    {
        internalValue = GameManager.Inventory.GetMoney();
        targetValue = internalValue;
        RefreshText();
    }

    private void Start()
    {
        UI_StoreEquipment.OnPurchase += LoadValue;
        UI_FullArmyDisplay.OnUpdateHero += LoadValue;
        UI_FullArmyDisplay.OnUpdateUnit += LoadValue;
        Inventory.OnInventoryUpdated += LoadValue;

        LoadValue();
    }

    private void OnDestroy()
    {
        UI_StoreEquipment.OnPurchase -= LoadValue;
        UI_FullArmyDisplay.OnUpdateHero -= LoadValue;
        UI_FullArmyDisplay.OnUpdateUnit -= LoadValue;
        Inventory.OnInventoryUpdated -= LoadValue;
    }

    private void Update()
    { 
        internalValue = Mathf.MoveTowards(internalValue, targetValue, countSpeed * Time.deltaTime);
        RefreshText();
    }

    private void RefreshText()
    {
        SetText($"{Mathf.RoundToInt(internalValue)}{suffix}");
    }

    public void LoadValue() => targetValue = GameManager.Inventory.GetMoney();
    public void LoadValue(object _) => targetValue = GameManager.Inventory.GetMoney();
}