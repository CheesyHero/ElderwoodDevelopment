using UnityEngine;

public class UI_GenerateItemRewards : MonoBehaviour
{
    public bool Rare { get { return GameManager.CurrentEncounter.depth >= 5; } }
    public UI_NewITemButton[] itemButtons;

    private void Awake()
    {
        itemButtons = GetComponentsInChildren<UI_NewITemButton>();
    }

    private void OnEnable()
    {
        Generate();
    }

    public void Generate()
    {
        Item[] pull = GameManager.CurrentEncounter.main.GetRewards(Rare);
        for (int i = 0; i < itemButtons.Length; i++)
        {
            itemButtons[i].PassObject(pull[i]);
        }
    }
}
