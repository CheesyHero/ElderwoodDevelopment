using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UI_UnitPreviewLoader : MonoBehaviour
{
    public Image header;
    public TextMeshProUGUI partyCount;
    public TextMeshProUGUI teamTitle;
    protected Unit lastInput = null;
    [SerializeField]
    protected UI_UnitSlot[] slots;

    protected virtual void OnEnable()
    {
        slots = GetComponentsInChildren<UI_UnitSlot>(); 
    }

    public virtual void LoadUnit(Unit unit, bool andDisable = true)
    {
        if (unit == null)
        {
            lastInput = null;
            SetActive(false);
            SetHeaderColor(Color.white);

            return;
        }

        lastInput = unit;

        if (andDisable) SetActive(true);
        ShowDisplays(unit);
        LoadTitle();
        LoadCount();
        SetHeaderColor(unit.team.teamColor);
    }

    protected void ShowDisplays(Unit unit)
    {
        if(unit == null) return;

        foreach (var item in slots)
            item.LoadUnit(unit);
    } 
    public void SetActive(bool setting)
    {
        gameObject.SetActive(setting);
    }
    public void PassUnit()
    {
        UI_FullArmyDisplay.SetNewCurrentUnit(lastInput);
    }
    public void LoadTitle()
    {
        if (teamTitle && lastInput != null)
            teamTitle.text = lastInput.ToString();
    }
    public void LoadCount()
    {
        if (partyCount && lastInput != null)
            partyCount.text = $"{lastInput.heroes.Count}/{lastInput.maxPlacements}";
    }

    public void SetHeaderColor(Color c)
    {
        if (header) header.color = c;
    }
}
