using System.Collections.Generic;
using TMPro; 
using UnityEngine;

public class UI_CurrentStackResolution : ComponentLoader
{
    public float printDuration = 3.0f;
    private float printTime = 0f;

    public float stickyDuration = 1.0f;
    private float stickyTime = 0f;

    public TextMeshProUGUI descriptionText;
    public TextMeshProUGUI weaponText;

    public GameObject handle;

    public Queue<BattleEvent> next = new();
    

    private void Awake()
    {
        if (!handle) handle = gameObject;

        AutoBattleManager.OnStackUpdated += PassObject;
    }
    private void OnDestroy()
    {
        AutoBattleManager.OnStackUpdated -= PassObject; 
    }

    private void Update()
    {
        if (stickyTime > 0) stickyTime -= Time.deltaTime;
        if (printTime > 0) printTime -= Time.deltaTime;

        if (next.Count > 0 && stickyTime <= 0)
        {
            PrintEntireAction(next.Dequeue());

            stickyTime = stickyDuration;
            printTime = printDuration;
        }
        else if (printTime < 0)
            handle.SetActive(false);

    }

    public override void PassObject(object obj)
    {
        if(obj is BattleEvent action)
        {
            next.Enqueue(action);
        }
        else handle.SetActive(false);
    }
    private void PrintEntireAction(BattleEvent action)
    {
        if (weaponText)
            weaponText.text = PrintWeaponText(action);

        SetText($"<b><u>{action.move.skillName}</u></b> | {BattleConstants.PrintMoveTypeWithColor(action.move)}");

        if (descriptionText)
            descriptionText.text = action.move.description;

        handle.SetActive(true); 
    }

    private string PrintWeaponText(BattleEvent action)
    {
        if (action.targets == null || action.targets.Count == 0)
            return $"~ {action.owner} | {action.owner.hero.equipment.weapon}";

        string target = action.owner == action.targets[0] ? "self" : 
            (action.targets[0] != null ? action.targets[0].ToString() : "Target");
        if (action.move.moveType == MoveType.action)
            return $"~ {action.owner} | {action.owner.hero.equipment.weapon} vs. {target}";

        else if (action.move.moveType == MoveType.reaction) 
            return $"{action.owner} targeting {target}"; 

        else // Special
            return $"{action.owner.hero.heroClass}";
    }
}
