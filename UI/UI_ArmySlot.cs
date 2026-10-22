using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ArmySlot : UI_UnitSlot, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Image targetImage;

    [Header("Colors")]
    [SerializeField] Color notPlaced = Color.gray;
    [SerializeField] Color placedHere = Color.green;
    [SerializeField] Color placedElsewhere = new (1f, 0.65f, 0f); // orange


    protected override void Awake()
    {
        base.Awake();

        UI_FullArmyDisplay.OnUpdateHero += GoRefresh;
        UI_FullArmyDisplay.OnUpdateUnit += GoRefresh;

        children = GetComponentsInChildren<ComponentLoader>();
    }
    private void OnDestroy()
    {
        UI_FullArmyDisplay.OnUpdateHero -= GoRefresh;
        UI_FullArmyDisplay.OnUpdateUnit -= GoRefresh;
    }
    public override void LoadHero(Hero hero, bool leader = false)
    {
        //Debug.Log($"{name} is loading new hero {hero}");

        base.LoadHero(hero, leader);
        Refresh(hero);
    } 

    public void GoRefresh(Hero _) => Refresh(lastInput);

    public void GoRefresh(Unit _) => Refresh(lastInput);
    public void Refresh(Hero hero)
    {
        if (hero == null || !targetImage)
            return;

        Unit foundUnit = GameManager.Player.FindExistingPlacedUnitOfHero(hero);

        if (foundUnit == null)
        {
            // Not currently placed
            targetImage.color = notPlaced;
        }
        else if (foundUnit == UI_FullArmyDisplay.CurrentUnit)
        {
            // Placed in current unit
            targetImage.color = placedHere;
        }
        else
        {
            // Placed in another unit
            targetImage.color = placedElsewhere;
        }
    }
}
