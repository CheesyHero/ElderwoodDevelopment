using UnityEngine;
using UnityEngine.Events;

public class CardHandler : MonoBehaviour
{  
    public Hand hand = new();
    public Deck deck = new();
    public Discard discard = new();
    public Exile exile = new();  
    public int Count { get { return deck.Count + hand.Count + discard.Count + exile.Count; } }
    public static UnityAction<CardHandler> OnDeckUpdated;

    protected virtual void Initialize(CommandCard[] source)
    {
        Clear();

        foreach (var card in source)
            deck.Push(new Card(card));
         
        deck.Shuffle();
    }
    public void ResetZones()
    {
        hand.MoveAllTo(deck);
        discard.MoveAllTo(deck);
        exile.MoveAllTo(deck);

        deck.Shuffle();
    }
    public void Clear()
    {
        deck.Clear();
        hand.Clear();
        discard.Clear();
        exile.Clear();
    }   
    public void AddToHand(Card c)
    {
        if (c != null)
            hand.Push(c);

        OnDeckUpdated?.Invoke(this);
    }
    public void AddToDeckTop(Card c)
    {
        if (c != null)
            deck.Top(c);

        OnDeckUpdated?.Invoke(this);
    }
    public void AddToDeckBottom(Card c)
    {
        if (c != null)
            deck.Bottom(c);

        OnDeckUpdated?.Invoke(this);
    }
    public void AddToDiscard(Card c)
    {
        if (c != null)
            discard.Push(c);

        OnDeckUpdated?.Invoke(this);
    }
    public void AddToExile(Card c)
    {
        if (c != null)
            exile.Push(c);

        OnDeckUpdated?.Invoke(this);
    }

    public void DrawCard()
    { 
        if(deck.Count <= 0)
            discard.MoveAllTo(deck);

        hand.Push(deck.Pop());

        OnDeckUpdated?.Invoke(this);
    }

    public void DrawCard(int count = 1)
    {
        for (int i = 0; i < count; i++)
            DrawCard();

        OnDeckUpdated?.Invoke(this);
    } 
    public void DiscardCard(Card c)
    {
        discard.Push(hand.Extract(c));

        OnDeckUpdated?.Invoke(this);
    }
    public void DiscardRandomCard()
    {
        Debug.LogWarning("DISCARDING A CARD");
        discard.Push(hand.PopRandom());

        OnDeckUpdated?.Invoke(this);
    }
    public void MillCard()
    {
        Debug.LogWarning("MILLING A CARD");
        discard.Push(deck.Pop());

        OnDeckUpdated?.Invoke(this);
    }
    public void ShuffleBackDiscard()
    {
        Debug.LogWarning("RESHUFFLING DECK");
        deck.Push(discard.Pop(discard.Count));
        deck.Shuffle();

        OnDeckUpdated?.Invoke(this);
    } 
}
