using UnityEngine;
using TMPro;

public class ItemTooltipPrinter : ComponentLoader
{
    public static ItemTooltipPrinter instance;

    private void Awake()
    {
        textMesh = gameObject.GetComponent<TextMeshProUGUI>();
        instance = this;
    }

    public override void PassObject(object obj)
    {
        if (obj is Item item)
            LoadItem(item);
    }

    void LoadItem(Item item) => SetText(item.PrintToolTip(true));

    public static void LoadToolTip(object obj)
    {
        if (instance == null) return;

        instance.PassObject(obj);
    }
}
