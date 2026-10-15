using UnityEngine;
using UnityEngine.UI;

public class UI_UnitIdentifier : MonoBehaviour
{
    public UI_UnitSlot unit;
    public Image image;

    private void Awake()
    {
        unit = GetComponentInParent<UI_UnitSlot>();

        UI_FullArmyDisplay.OnUpdateHero += Check;
        UI_FullArmyDisplay.OnUpdateUnit += Check;
    }
    private void OnEnable()
    {
        Check();
    }
    private void OnDestroy()
    {
        UI_FullArmyDisplay.OnUpdateHero -= Check;
        UI_FullArmyDisplay.OnUpdateUnit -= Check;
    }

    public void Check(object _ = null)
    {
        if (!unit) return;

        bool matches = UI_UnitSlot.LastIndexMatches(unit);

        image.enabled = matches || CurrentIsNotNull(); 
    }
    public bool CurrentIsNotNull()
    { 
        return UI_FullArmyDisplay.CurrentHero != null && UI_FullArmyDisplay.CurrentHero == unit.GetLastInput();
    }
}
