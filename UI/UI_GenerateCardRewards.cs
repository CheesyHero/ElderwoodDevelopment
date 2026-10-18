using UnityEngine;

public class UI_GenerateCardRewards : MonoBehaviour
{
    public bool Rare { get { return GameManager.CurrentEncounter.depth >= 5; } }
    public UI_CardRewardButton[] cardButtons;

    private void Awake()
    {
        cardButtons = GetComponentsInChildren<UI_CardRewardButton>();
    }

    private void OnEnable()
    {
        Generate();
    }

    public void Generate()
    {
        CommandCard[] pull = GameManager.CurrentEncounter.main.GetNewCards(Rare);
        for (int i = 0; i < cardButtons.Length; i++)
        {
            cardButtons[i].LoadCommand(pull[i]);
        }
    }
}
