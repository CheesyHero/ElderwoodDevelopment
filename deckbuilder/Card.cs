using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Events;

 
[System.Serializable]
public class Card
{
    public Army owner;
    public CommandCard command; 

    public static UnityAction<Card, Zone> OnZoneEntered;
    public static UnityAction<Card, Zone> OnZoneLeft;

    public Card(CommandCard card)
    { 
        command = card;
    }
    public void Resolve(object next)
    {
        if (command) command.Resolve(next, owner);
    }

    public void EnteredZone(Zone zone)
    {
        //Debug.Log("Card: " + ToString() + " moved to new zone: " + zone);
        OnZoneEntered?.Invoke(this, zone);
    }
    public void ExitedZone(Zone zone)
    {
        //Debug.Log("Card: " + ToString() + " exited from old zone: " + zone);
        OnZoneLeft?.Invoke(this, zone);
    } 
    public override string ToString()
    {
        return command ? command.ToString() : "Empty Card";
    }
    public void SetOwner(Army source)
    {
        owner = source;
    } 
    public Card Copy(bool _ = false)
    {
        Card newCard = new(command); 

        return newCard;
    }
}

[System.Serializable]
public class TargetingData
{
    public bool isCopy = false;
    public CardHandler owner;
    public CardHandler target;
    public int stackIndexTarget = -1;

    public TargetingData()
    {
        isCopy = false;
        owner = null;
        target = null;
        stackIndexTarget = -1;
    }

    public TargetingData(TargetingData copy)
    {
        isCopy = copy.isCopy;
        owner = copy.owner;
        target = copy.target;
        stackIndexTarget = copy.stackIndexTarget;
    }
}
