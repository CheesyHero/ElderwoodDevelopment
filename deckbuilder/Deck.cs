using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Deck : Zone
{
    public Deck()
    {
        zoneName = "deck";
    }
     
    public virtual void SetOwnership(Army source)
    {
        foreach (var item in cards)
            item.SetOwner(source);
    }
    public void Top(Card card)
    {
        cards.Insert(0, card);
        card.EnteredZone(this);
    }
    public void Bottom(Card card)
    {
        Push(card);
    }
}
