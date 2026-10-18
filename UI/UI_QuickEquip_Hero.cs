using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class UI_QuickEquip_Hero : MonoBehaviour, IPointerEnterHandler
{
    public Image heroIcon;
    public TextMeshProUGUI heroName;
    public TextMeshProUGUI heroLevel;

    private Hero hero;
    public void OnPointerEnter(PointerEventData eventData)
    {
        UI_QuickEquipMenu.LoadPreview(hero);
    }

    public void PassObject(Hero input)
    {
        hero = input;

        if (hero == null) 
            Clear();
        else
        {
            heroName.text = hero.characterName;
            heroLevel.text = $"Lv. {hero.level} {hero.heroClass.className}";
            heroIcon.sprite = hero.heroClass.actorImage;
            heroIcon.color = Color.white;
        }

    }

    public void Clear()
    {
        hero = null;
        heroName.text = "";
        heroLevel.text = "";
        heroIcon.sprite = null;
        heroIcon.color = Color.clear;
    }

    public Hero GetHero()
    {
        return hero;
    }

    public void Commit()
    {
        UI_QuickEquipMenu.Commit(hero);
    }
}