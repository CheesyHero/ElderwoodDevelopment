using System.Collections.Generic; 
using UnityEngine;

[System.Serializable]
public class Zone
{
    public int maxSize = 99;

    public Zone()
    {
        zoneName = "new";
    }
    public override string ToString()
    {
        string s = char.ToUpper(zoneName[0]) + zoneName.Substring(1).ToLower();
        return string.IsNullOrEmpty(zoneName) ? base.ToString() : s + " Zone";
    }

    protected string zoneName = "zone";
    [SerializeField]
    protected List<Card> cards;
    public Card[] Cards { get { return cards.ToArray(); } }
    public int Count
    { get { return cards.Count; } }

    public virtual Card[] Copy()
    {
        return cards.ToArray();
    }
    public virtual void Push(Card card)
    {
        if (card == null || cards.Count >= maxSize) return;

        cards.Add(card);
        card.EnteredZone(this);
    }
    public virtual void Push(Card[] list)
    {
        if (cards.Count + list.Length - 1 >= maxSize) return;

        foreach (var item in list)
        {
            Push(item);
            item.EnteredZone(this);
        }
    }
    public virtual Card Pop()
    {
        if (cards.Count == 0) return null;

        var t = cards[0];
        cards.RemoveAt(0);
        t.ExitedZone(this);
        return t;
    }
    public virtual Card[] Pop(int count)
    {
        List<Card> gets = new();
        count = Mathf.Min(count, cards.Count);

        for (int i = 0; i < count; i++)
        {
            var t = Pop();
            gets.Add(t);
            t.ExitedZone(this);
        }

        return gets.ToArray();
    }
    public virtual Card Extract(Card card)
    {
        int i = cards.IndexOf(card);
        return Extract(i);
    }
    public virtual Card Extract(int index)
    {
        if (index < 0 || index >= Count)
            return null;

        Card get = cards[index];
        cards.RemoveAt(index);
        get.ExitedZone(this);

        return get;
    }
    public virtual Card Peek(int index = 0)
    {
        if (cards.Count == 0) 
            return null;

        index = Mathf.Clamp(index, 0, Count);
        return cards[index];
    } 
    public virtual bool Contains(Card card)
    {
        return cards.Contains(card);
    } 
    public virtual void Shuffle()
    {
        List<Card> shuffledDeck = new();

        while(cards.Count > 0)
        {
            int r = Random.Range(0, cards.Count);
            shuffledDeck.Add(cards[r]);
            cards.RemoveAt(r);
        }
         
        cards.AddRange(shuffledDeck.ToArray());
    }

    public virtual bool Move(Card card, Zone other)
    {
        if (other == null || card == null) 
            return false;

        var find = Extract(card); 
        if (find != null)
        {
            other.Push(find); 
            return true;
        } 
        else return false;
    }

    public virtual Card PopRandom()
    {
        int r = Random.Range(0, Count);
        var c = Extract(r); 
        return c;
    }

    public virtual void Clear()
    {
        cards.Clear();
    }
    public void MoveAllTo(Zone other)
    {
        while(cards.Count > 0) 
            Move(cards[0], other);
    }

    public void AddCard(Card newCard)
    {
        if (newCard == null) return;

        cards.Add(newCard);
    }

    public void Sort(int index)
    {
        switch (index)
        {
            default:
                // Sort by name
                break;

            case 1:
                // Sort by cost
                break;

            case 2:
                // Sort by rarity
                break; 
        }
    }
}
