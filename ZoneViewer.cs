using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ZoneViewer : MonoBehaviour
{
    public Zone LastZone { get; private set; }
    [SerializeField]
    protected CardViewer[] children;

    public void AssignLoadedChildren() => children = GetComponentsInChildren<CardViewer>();
    private void Awake()
    {
        AssignLoadedChildren();

        PlayerHandZone.OnCardPlayed += ReloadLastZone;
        CardHandler.OnDeckUpdated += ReloadLastZone;
        TurnManager.OnEventTrigger += ReloadLastZone;
    }

    private void OnDestroy()
    {
        PlayerHandZone.OnCardPlayed -= ReloadLastZone;
        CardHandler.OnDeckUpdated -= ReloadLastZone;
        TurnManager.OnEventTrigger -= ReloadLastZone;
    }

    public void ReloadLastZone() => LoadZone(LastZone);
    public void ReloadLastZone(string _) => ReloadLastZone();
    public void ReloadLastZone(Card _) => ReloadLastZone();
    public void ReloadLastZone(CardHandler _) => ReloadLastZone();

    public void LoadZone(Zone zone)
    {
        // Views all cards in desired zone
        if (zone == null) return;

        HideAllChildren();

        for (int i = 0; i < zone.Cards.Length; i++) 
            LoadButtonAtIndex(i, zone.Cards[i]); 

        LastZone = zone;
    }

    public void LoadButtonAtIndex(int index, Card card)
    {
        if (index < 0 || index > children.Length) return;
         
        children[index].LoadCard(card);
    }
    public void HideAllChildren()
    {
        foreach (var item in children)
            item.Hide();
    }
}
