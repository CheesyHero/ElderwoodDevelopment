using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_HeroPreviewPrinter : MonoBehaviour
{
    public HeroClass reference;
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI content;

    private void Start()
    {
        PrintHero(reference);
    }

    public void PrintHero(HeroClass hero)
    {
        if (!hero) return;

        if (icon) icon.sprite = hero.actorImage;
        if (title) title.text = hero.className;
        if (content) content.text = " - " + hero.description;
    }
}
