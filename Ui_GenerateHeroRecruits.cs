using UnityEngine;

public class Ui_GenerateHeroRecruits : MonoBehaviour
{
    public UI_AddNewHero[] heroButtons;

    private void Awake()
    {
        heroButtons = GetComponentsInChildren<UI_AddNewHero>();
    }

    private void OnEnable()
    {
        Generate();
    }

    public void Generate()
    {
        var pull = GameManager.CurrentEncounter.main.GetRandomHeroes();
        for(int i = 0; i < heroButtons.Length; i++)
        {
            heroButtons[i].PassObject(pull[i]);
        }
    }
}
