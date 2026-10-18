using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_UnitButton : ComponentLoader, IPointerEnterHandler, IPointerExitHandler
{
    private Unit lastUnit = null;

    public UnityAction<int> OnButtonPressed;

    public Button button;
    public int index = -1;

    [Header("Helper Colors")]
    public Color inactiveColor = Color.white;
    public Color activeColor = Color.white;
    public Color defeatedColor = Color.white;

    private void Awake()
    {
        button = GetComponent<Button>();

        EnemyArmyManager.OnUnitSpawned += Refresh;
        Selectable.OnStateChange += Refresh;
        HexWalker.OnVisit += Refresh;
        HexWalker.OnWalk += Refresh;
    }

    private void OnDestroy()
    {
        EnemyArmyManager.OnUnitSpawned -= Refresh;
        Selectable.OnStateChange -= Refresh;
        HexWalker.OnVisit -= Refresh;
        HexWalker.OnWalk -= Refresh; 
    }

    public override void PassObject(object obj)
    {
        if (obj is not Unit unit)
            return;

        if (unit.heroes.Count <= 0)
            return;

        SetActive(true);

        button.interactable = unit.IsAlive();

        int cost = GameManager.GetPlayerSpawnCost(unit);

        if (unit.IsAlive())
        {
            HexWalker walker = HexWalker.Exists(unit);

            if (walker)
                SetText($"{unit.Leader()} : ({walker.moves} moves)");
            else
                SetText($"{unit.Leader()} : ({cost} CP)");
        }
        else
        {
            SetText($"{unit.Leader()} : Cooldown {unit.Cooldown}");
        }

        lastUnit = unit;
        if (lastUnit == null) return;

        Color color = HexWalker.Exists(GameManager.Player, index) ? activeColor : inactiveColor;

        var cb = new ColorBlock
        {
            normalColor = color * 0.85f,
            highlightedColor = color,
            pressedColor = color * 0.7f,
            selectedColor = color,
            disabledColor = defeatedColor,
            colorMultiplier = 1f,
            fadeDuration = 0.1f
        };

        button.colors = cb;

        button.enabled = lastUnit.Cooldown <= 0;
    }

    public void Refresh() => PassObject(lastUnit);

    public void Refresh(Selectable _) => Refresh();

    public void SetIndex(int i)
    {
        index = i;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (HexWalker.Exists(lastUnit) == false)
            UI_CommandPoints.ShowPreview(GameManager.GetPlayerSpawnCost(lastUnit));

        MapUnitPreviewer.LoadPreview(lastUnit);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        UI_CommandPoints.EndPreview();

        MapUnitPreviewer.ClosePreview();
    }

    public void BUTTON_PassIndex()
    {
        PlayerSelection.Cancel();

        OnButtonPressed?.Invoke(index);

        HexWalker exists = HexWalker.Exists(lastUnit);

        if (exists == null)
        {
            int cost = GameManager.GetPlayerSpawnCost(lastUnit);

            if (GameManager.Player.commandPoints < cost)
            {
                BannerArray.PrintReport($"<color=red>Not enough Command Points:</color> You need {cost} points.");
                return;
            }

            BannerArray.PrintReport($"Attempting to place {lastUnit}...");

            var hit = HexCrawlManager.GetTeamBaseByIndex(TurnManager.Turn); 
            if (hit) CameraController.GoToClosestBase(lastUnit.team);

            TileSpawnValidator.ShowValid(GameManager.Player);
        }
        else
        {
            BannerArray.PrintReport($"Finding {lastUnit}...");
            CameraController.MoveToGameObject(exists.gameObject, true);
        }
    }
}