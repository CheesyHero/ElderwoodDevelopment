using System;
using System.Collections;
using System.Collections.Generic; 
using UnityEngine;
 
[SelectionBase]
public class BattleTile : MonoBehaviour
{ 

    [SerializeField] 
    protected Vector2 unitPosition;
    public BattleCombatant CurrentHero { get; private set; }
    public BattleCombatant SetHero(BattleCombatant newHero, Hero input, Team team, bool flip = true)
    {
        if (input.health.IsDead)
            return null;

        var spawn = Instantiate(newHero, transform.position, transform.rotation, transform); 

        CurrentHero = spawn;
        CurrentHero.hero = input;
        CurrentHero.team = team;

        CurrentHero.Initialize();
        CurrentHero.FindArt(flip);

        CurrentHero.name = CurrentHero.FullTeamName;
        CurrentHero.placement = unitPosition;

        return CurrentHero;
    }

    private void Awake()
    {
        SetUnitPosition();
    }
    public void SetUnitPosition()
    {
        unitPosition = GetUnitPosition();
    } 
    public Vector2 GetUnitPosition()
    {  
        return unitPosition;
    }
}
