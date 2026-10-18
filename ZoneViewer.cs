using System.Collections.Generic;
using UnityEngine;

public class ZoneViewer : MonoBehaviour
{
    public Zone LastZone { get; private set; }
    public CardViewer prefab;
    public Transform handle;

    [SerializeField]
    protected List<CardViewer> children = new();

    public void AssignLoadedChildren()
    {
        children = new List<CardViewer>(
            GetComponentsInChildren<CardViewer>(true)
        );
    }

    private void Awake()
    {
        AssignLoadedChildren();

        PlayerHandZone.OnCardPlayed += ReloadLastZone;
        CardHandler.OnDeckUpdated += ReloadLastZone;
        TurnManager.OnEventTrigger += ReloadLastZone;

        if (handle == null) handle = transform;
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
        if (zone == null) return;

        Card[] cards = zone.Copy();

        // Create enough viewers for every card
        while (children.Count < cards.Length)
        {
            CardViewer viewer = Instantiate(prefab, handle);
            children.Add(viewer);
        }

        // Disable excess viewers, starting from the end
        for (int i = children.Count - 1; i >= cards.Length; i--)
        {
            children[i].gameObject.SetActive(false);
        }

        // Populate all required viewers
        for (int i = 0; i < cards.Length; i++)
        {
            children[i].gameObject.SetActive(true);
            children[i].LoadCard(cards[i]);
        }

        LastZone = zone;
    }

    public void LoadButtonAtIndex(int index, Card card)
    {
        if (index < 0 || index >= children.Count) return;

        children[index].gameObject.SetActive(true);
        children[index].LoadCard(card);
    }

    public void HideAllChildren()
    {
        foreach (var child in children)
            child.gameObject.SetActive(false);
    }
}