using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerHandZone : ZoneViewer
{ 
    public DeckManager Player { get { return GameManager.Player.deck; } }
    public static Card NextCard { get; private set; }

    public static UnityAction<Card> OnCardPlayed;
     
    private void OnEnable()
    {
        if (!Player) return;

        LoadZone(Player.hand);
        foreach (var button in children)
            button.onButtonPressed = PreparCardTargets;
    }
    private void Resolve(object push) => NextCard?.Resolve(push);

    public void PreparCardTargets(Card next)
    {
        if (next == null || TurnManager.IsTurn(GameManager.Player) == false) return;

        NextCard = next;

        PlayerSelection.Cancel();

        if(GameManager.Player.commandPoints >= NextCard.command.cost) 
            AwaitNextSelection();

        else BannerArray.PrintReport($"<color=red> Not enough Command Points</color> for card: {NextCard.command}");
    }
    public void AwaitNextSelection()
    {
        if (NextCard == null) return;

        BannerArray.PrintReport($"<Color=yellow>LOAD NEW CARD</color> Player is attempting to play card {NextCard.command}");
        // Here now, we can decide what type of selection we are expecting
        switch (NextCard.command.targetType)
        {
            default: return;

            case E_CommandTargetType.None:
                return; // Do nothing

            case E_CommandTargetType.TargetActiveUnit:
                BannerArray.PrintReport($"Select a valid unit for {NextCard}");
                PlayerSelection.selectionOverride = SelectHexWalker;
                break; // If a HexWalker is passed through, we are happy

            case E_CommandTargetType.targetTile:
                BannerArray.PrintReport($"Select a valid tile for {NextCard}");
                PlayerSelection.selectionOverride = SelectHexTile;
                break; // If a HexTile is passed through, we are happy

            case E_CommandTargetType.targetBase:
                BannerArray.PrintReport($"Select a valid base for {NextCard}");
                PlayerSelection.selectionOverride = SelectTeamBase;
                break; // If a TeamBase is passed through, we are happy

            case E_CommandTargetType.cardInZone:
                ZoneViewerSelector.LoadWindow(Player.GetZone(NextCard.command.fromZone));

                break; // Opens up a window appropriate for the zone

            case E_CommandTargetType.Owner:
                break; // Selects the user for the effect // Opens a confirm menu

            case E_CommandTargetType.TargetOpponent:
                break; // opens a window that shows all opponents

            case E_CommandTargetType.TargetPlayer:
                break; // opens a window that shows all participants

            case E_CommandTargetType.TargetHeroInTargetUnit:
                break; // Works like, Select Walker, but when a walker is selected it opens a window to select a hero within it

            case E_CommandTargetType.TargetInactiveUnit:
                break; // Opens a window for the player's inactive units (EX: Reviving a defeated unit)
        }
    }

    public void UseCard()
    {
        GameManager.Player.UseCommandPoints(NextCard.command.cost);
        Player.DiscardCard(NextCard);

        NextCard = null; 
        ReloadLastZone(); 

        PlayerSelection.Cancel();
        OnCardPlayed?.Invoke(NextCard);
    }
    private void SelectHexWalker(Selectable selection)
    {
        if(NextCard == null) return;
        if(selection is HexWalker walker)
        {
            Resolve(walker);
            UseCard();
        } 
    }

    private void SelectHexTile(Selectable selection)
    {
        if (NextCard == null) return;
        if (selection is HexTile tile)
        {
            Resolve(tile);
            UseCard();
        }
    }

    private void SelectTeamBase(Selectable selection)
    {
        if (NextCard == null) return;
        if (selection is TeamBase stronghold)
        {
            Resolve(stronghold);
            UseCard();
        }
    }

}
