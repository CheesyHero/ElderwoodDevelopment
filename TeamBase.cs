using UnityEngine;
using System.Collections.Generic;
using System;

[SelectionBase]
public class TeamBase : HexTile
{
    public static readonly List<TeamBase> bases = new(); 

    [Header("Base Info")]
    public string strongholdName = "Castle";
    public int index = -1; // no team

    public ParticleSystem CaptureFX;
     
    public override void InitiateStartState()
    {
        base.InitiateStartState();
        
        foreach(var item in adjacents)
        {
            if (item.CurrentUnit == null)
            {
                item.owner = owner;
                item.ChangeState(item.state);
            }
        }
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        bases.Add(this);
    }
    protected override void OnDisable()
    {
        base.OnDisable();
        bases.Remove(this);
    }
    protected override void OnDestroy()
    {
        base.OnDestroy();
        bases.Remove(this);
    }
    public override void SetInitialOwner()
    {
        if (owner.teamName.Length == 0)
            owner = new("Neutral", Color.white);
    }
    public override string ToString()
    {
        return strongholdName.Length > 0 ? owner + " " + strongholdName : base.ToString();
    }

    public void LoadTeam()
    {
        var teams = EnemyArmyManager.AllArmies;
        if (teams.Length > index && index >= 0)
            Capture(teams[index], false);
    }
    public static void LoadAllTeams()
    {
        foreach(var item in bases)
            item.LoadTeam();
    }
    public override void Capture(Army army, bool broadcast)
    {
        if (army.team == owner) return;

        base.Capture(army, broadcast);
        foreach (var item in adjacents)
            if (item.CurrentUnit == null)
                item.Capture(army, broadcast);

        if (broadcast)
        { 
            BannerArray.PrintReport($"{strongholdName} has been captured by {owner}");
            CameraController.MoveToGameObject(gameObject, false);

            army.GainCommandPoints(1);
            army.DrawCards(1);

            HexCrawlManager.CheckWinner();

            if (CaptureFX)
                Instantiate(CaptureFX, transform.position, transform.rotation, transform);
        }
    }

    public static bool HasDominion(Team team)
    { 
        foreach(var item in bases)
        {
            if (item.owner != team)
                return false;
        }

        return true;
    }

    public static int DominionCount(Team team)
    {
        int total = 0;
        foreach(var item in bases)
        {
            if (item.owner == team)
                total++;
        }

        return total;
    }
}
