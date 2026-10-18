using UnityEngine;
using TMPro;

public class ShopCardViewer : CardViewer
{
    public Transform handle;
    public TextMeshProUGUI money;
    [Range(0, 9)]
    public int shopIndex = 0;
     
    public CommandCard Card 
    { 
        get 
        {  
            if(shopIndex < 0 ||  shopIndex > GameManager.CurrentEncounter.cardMarket.Count)
                return null;

            return shopIndex < GameManager.CurrentEncounter.cardMarket.Count ? GameManager.CurrentEncounter.cardMarket[shopIndex] : null; 
        } 
    }

    private void OnEnable()
    { 
        LoadCommand(Card);
    }

    public void BUTTON_Purchase()
    {
        Debug.Log("PURCHASE ATTEMPT START");

        if (Card == null) return;

        CommandCard card = Card;
        Debug.Log($"Attempting to purchase card : {card}");


        if (GameManager.Player.inventory.CanSpend(card.money))
        {
            GameManager.CurrentEncounter.cardMarket[shopIndex] = null;

            GameManager.Player.inventory.SpendMoney(card.money);
            GameManager.Player.deck.deck.AddCard(new(card));

            BannerArray.PrintReport($"{card.cardName} has been added to your commands.");
        }
        else Debug.Log($"Player has insuffecient funds : {GameManager.Inventory.GetMoney()} / {card.cost}");

        LoadCommand(null);
    }

    public override void LoadCommand(CommandCard card)
    {
        base.LoadCommand(card);

        string color = card ? GameManager.Inventory.CanSpend(card.money) ? "<color=yellow>" : "<color=red>" : "";
        if (money) money.text = card ? $"{color}{card.money:0 Gold}</color>" : "<color=grey>SOLD OUT</color>";

        if (handle) handle.gameObject.SetActive(card != null); 
    }
}
