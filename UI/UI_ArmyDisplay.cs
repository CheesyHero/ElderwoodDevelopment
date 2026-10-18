using UnityEngine;
using System.Collections.Generic;

public class UI_ArmyDisplay : MonoBehaviour
{
    [SerializeField] public UI_ArmySlot prefab;
    protected UI_ArmySlot[] slots;

    private void Awake()
    {
        RefreshSlots();

        UI_FullArmyDisplay.OnUpdateUnit += LoadPlayer;
        UI_FullArmyDisplay.OnUpdateHero += LoadPlayer;
    }

    private void OnDestroy()
    {
        UI_FullArmyDisplay.OnUpdateUnit -= LoadPlayer;
        UI_FullArmyDisplay.OnUpdateHero -= LoadPlayer;
    }

    private void Start()
    {
        LoadPlayer();
    }

    public void LoadPlayer(object _ = null) => LoadArmy(GameManager.Player);

    public void LoadArmy(Army army)
    {
        army.SortPartyByUnits();

        EnsureSlotCount(army.party.Count);

        for (int i = 0; i < slots.Length; i++)
        {
            UI_ArmySlot item = slots[i];

            if (i < army.party.Count)
            {
                item.gameObject.SetActive(true);

                Hero h = army.party[i];
                item.LoadHero(h, army.IsUnitLeader(h));
            }
            else
            {
                item.gameObject.SetActive(false);
            }
        }
    }

    private void EnsureSlotCount(int requiredCount)
    {
        if (slots != null && slots.Length >= requiredCount)
            return;

        int currentCount = slots == null ? 0 : slots.Length;

        for (int i = currentCount; i < requiredCount; i++)
        {
            UI_ArmySlot newSlot = Instantiate(prefab, transform);
            newSlot.name = $"{prefab.name} ({i})";
            ModifyScaleInDelay(newSlot, i);
        }

        RefreshSlots();
    }

    private void RefreshSlots()
    {
        slots = GetComponentsInChildren<UI_ArmySlot>(true);

        for (int i = 0; i < slots.Length; i++)
            ModifyScaleInDelay(slots[i], i);
    }

    public bool IsUnitLeader(Hero hero, Army army)
    {
        return army.IsUnitLeader(hero);
    }

    public void ModifyScaleInDelay(UI_ArmySlot button, float step)
    {
        if (button.TryGetComponent<ScaleIn>(out var find))
            find.delay = step / 100f;
    }
}