using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class CardViewer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Card lastInput;
    public CommandCard lastCommand;
    public delegate void OnButtonPressed(Card send);
    public OnButtonPressed onButtonPressed;

    public TextMeshProUGUI title;
    public TextMeshProUGUI cost;
    public TextMeshProUGUI description;

    public Image artDisplay;
    public Image costIcon;
    public Image boarderColor;

    public Button button;

    public void Display()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
    public virtual void Show()
    {
        if(lastInput == null)
        {
            Hide();
            return;
        }
        else
        {
            LoadCommand(lastInput.command);
            Display();
        } 
    }
    public virtual void LoadCommand(CommandCard card)
    {
        lastCommand = card; 

        if (title) title.text = card ? card.cardName : "<color=red>SOLD OUT</color>";
        if (cost) cost.text = card ? card.cost.ToString("0") : "--";
        if (description) description.text = card ? card.description : "<color=grey>OUT OF STOCK</color>";
        if (artDisplay) artDisplay.sprite = card ? card.image : null;
        if (costIcon) costIcon.color = card ? BattleConstants.GetBadgeColor(card.rarity) : Color.gray2;
        if (boarderColor) boarderColor.color = card ? BattleConstants.GetBadgeColor(card.rarity) : Color.gray2;
    }

    public virtual void LoadCard(Card next)
    {
        lastInput = next;

        if (lastInput != null) Show();
        else Hide();
    }
    public void BUTTON_Press()
    {
        onButtonPressed?.Invoke(lastInput);
    } 
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (lastCommand != null)
            UI_CommandPoints.ShowPreview(lastCommand.cost);
    }
    public void OnPointerExit(PointerEventData eventData)
    { 
            UI_CommandPoints.EndPreview();
    }

}
