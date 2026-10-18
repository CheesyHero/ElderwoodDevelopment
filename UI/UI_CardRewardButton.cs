using UnityEngine;

public class UI_CardRewardButton : CardViewer
{
    public void BUTTON_AddCard()
    {
        if (lastCommand == null) return;
         
        GameManager.Player.deck.AddToDeckTop(new(lastCommand));
        lastCommand = null;
    }
}
