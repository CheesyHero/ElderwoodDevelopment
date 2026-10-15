using UnityEngine; 

public enum E_CommandTargetType
{
    None = 0, TargetActiveUnit = 1, TargetInactiveUnit = 2, // Reviving defeated units
    TargetHeroInTargetUnit = 3,

    targetTile = 5, targetBase = 6,

    Owner = 11, TargetOpponent = 12, TargetPlayer = 13,

    cardInZone = 20,
}
public enum E_PickCardTarget
{
    self, opponent, anyPlayer
}
public enum E_PickCardFromZone
{
    None = 0, Hand = 1, Discard = 2, Exile = 3,
}

[CreateAssetMenu(fileName = "New Command", menuName = "NMD/CommandEffects")]
public class CommandCard : ScriptableObject
{
    public int money = 100;
    public string cardName = "newCard";
    public RarityOptions rarity;
    public Sprite image;
    [TextArea(2, 6)]
    public string description = "A new card that does new things, but sometimes doesnt.";
    [Header("Economy")]
    [Range(0, 10)]
    public int cost = 2;
    public CommandEffects[] effects;

    [Header("Move Card Selection")]
    public E_CommandTargetType targetType;
    [Tooltip("Only for selecting cards.")]
    public E_PickCardTarget target;
    [Tooltip("Only for selecting cards.")]
    public E_PickCardFromZone fromZone;
    [Tooltip("Only for selecting cards.")]
    public E_PickCardFromZone toZone;

    [Header("SFX")] 
    public ParticleSystem particleEffect;

    public override string ToString() => cardName.Length > 0 ? cardName : base.ToString(); 

    public bool Resolve(object target, Army owner) => Resolve(new[] { target }, owner);
    public bool Resolve(object[] targets, Army owner)
    {
        if (targets == null || targets.Length <= 0) return false;

        // We want to be able to pass anything through the resolution
        // However, different effects will require different targets

        // EXAMPLE: Deal 30 damage to each hero in Target Unit
        // EXA MPLE: Restore 50% health to all heroes in Target Unit
        // EXAMPLE: Defeat Target Non-Leader Hero inside Target Unit
        // EXAMPLE: Return Target Card from your Discard pile back to your hand
        // EXAMPLE: Look at and pick a card from Target Opponent's hand. They discard that card.

        Debug.Log($"Attempting to resolve {this}...");


        return targetType switch
        {
            E_CommandTargetType.TargetActiveUnit => PushWalker(targets[0] as HexWalker, owner),
            E_CommandTargetType.TargetInactiveUnit => PushUnit(targets[0] as Unit),
            E_CommandTargetType.TargetHeroInTargetUnit => PushHero(targets[0] as Hero),

            E_CommandTargetType.targetTile => PushTile(targets[0] as HexTile),
            E_CommandTargetType.targetBase => PushTile(targets[0] as TeamBase),

            E_CommandTargetType.Owner => PushArmy(targets[0] as Army),
            E_CommandTargetType.TargetOpponent => PushArmy(targets[0] as Army),
            E_CommandTargetType.TargetPlayer => PushArmy(targets[0] as Army),

            E_CommandTargetType.cardInZone => PushCardInZone(targets),

            _ => false,// Do nothing if None is selected
        };
    }

    private bool PushWalker(HexWalker walker, Army source)
    {
        if (walker == null) return false;

        if (particleEffect)
            Instantiate(particleEffect, walker.transform.position, walker.transform.rotation, walker.transform);

        foreach (var item in effects)
            item.ApplyToWalker(walker);
         
        walker.Defeat(source);

        return true;
    }

    private bool PushTile(HexTile tile)
    {
        if (tile == null) return false;

        // Effect the WALKER here
        Debug.LogWarning("PUSH TILE IS INCOMPLETE");

        return true;
    }

    private bool PushUnit(Unit unit)
    {
        if (unit == null) return false;
         
        foreach (var item in effects)
            item.ApplyToUnit(unit);

        return true;
    }
    private bool PushHero(Hero hero)
    {
        if (hero == null) return false;

        foreach (var item in effects)
            item.ApplyToHero(hero);

        return true;
    }

    private bool PushArmy(Army army)
    {
        if (army == null) return false;

        // Effect the ARMY here
        Debug.LogWarning("PUSH ARMY IS INCOMPLETE");

        return true;
    }

    private bool PushCardInZone(object[] targets)
    {
        if (targets == null || targets.Length != 3) return false;

        return PushCardInZone(targets[0] as Card, targets[1] as Zone, targets[2] as Zone);
    }

    private bool PushCardInZone(Card card, Zone zone, Zone next)
    {
        if (card == null || zone == null || next == null) 
            return false; // For moving a card from zone A to zone B

        if (zone.Contains(card))
            zone.Move(card, next);

        return true; // Confirm that the move worked
    }
}
