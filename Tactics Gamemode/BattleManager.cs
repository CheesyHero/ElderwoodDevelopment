using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    protected static BattleManager instance;
    [SerializeField]
    protected Stack stack = new();

    private void Start()
    {
        instance = this;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            Resolve();
        if (Input.GetKeyDown(KeyCode.W))
            CopySpell();
    }

    public static void AddToStack(Card card, Zone source = null)
    {
        if (instance) instance.stack.Push(card);
        else source?.Push(card);
    }
    public static void Resolve()
    {
        Card c = instance.stack.Pop();
        if (c == null) return; 
    }
    public static void CopySpell(int index = 0)
    {
        Card c = instance.stack.Peek(index).Copy(true); 
        AddToStack(c);
    }
}
