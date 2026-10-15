using UnityEngine;

public class UI_ShowAvailableMoves : MonoBehaviour
{
    UI_MoveDisplay[] buttons;

    private void Awake()
    {
        buttons = GetComponentsInChildren<UI_MoveDisplay>();
    }
    private void OnEnable()
    {
        LoadCurrentMoves();
    }
    public void LoadCurrentMoves()
    {
        CloseButtons();

        if (UI_FullArmyDisplay.CurrentHero == null) return;

        var list = UI_FullArmyDisplay.CurrentHero.KnownSkills(); 
        if (list.Count <= 0) return;

        for(int i = 0; i < list.Count; i++)
        { 
            buttons[i].PassObject(list[i]);
            buttons[i].SetActive(true);
        }
    }
    public void CloseButtons()
    {
        foreach(var item in buttons) 
            item.gameObject.SetActive(false);
    }
}
