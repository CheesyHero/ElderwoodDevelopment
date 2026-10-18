using UnityEngine;

public class DeckManager : CardHandler
{
    public override string ToString()
    {
        return parent ? parent.team.ToString() : base.ToString();
    }

    [Header("Manager Data")]
    public Army parent;  

    public void LoadDeck(Army source)
    {
        if(source != null && source.generationList)

        parent = source;
        deck.SetOwnership(parent);

        var p = parent ? parent.generationList : null;

        if (p == null) return;
        else Initialize(p.commands);
    }

    public Zone GetZone(E_PickCardFromZone zone)
    {
        return zone switch
        {
            E_PickCardFromZone.Hand => hand,
            E_PickCardFromZone.Discard => discard,
            E_PickCardFromZone.Exile => exile,
            E_PickCardFromZone.None => null,
            _ => hand,
        };
    }

    public void ResetDeck()
    {
        hand.MoveAllTo(deck);
        discard.MoveAllTo(deck);
        exile.MoveAllTo(deck);

        deck.Sort(0);
    }
}
