using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Stack : Zone
{ 
    public Stack()
    {
        zoneName = "stack";
    }

    public override void Push(Card card)
    {
        if (card == null) return;
        cards.Insert(0, card);
    }
    public override void Push(Card[] list)
    {
        foreach(var item in list) 
            Push(item);
    }
    public override Card Pop()
    {
        return Extract(0); 
    }
    public override Card[] Pop(int x)
    {
        List<Card> gets = new();
        x = Mathf.Min(x, Count);

        for(int i = 0; i < x; i++)
            gets.Add(Pop());

        return gets.ToArray();
    }
}
