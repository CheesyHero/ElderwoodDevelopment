using UnityEngine;
using UnityEngine.UI; 

public class UI_TeamHealth : ComponentLoader
{ 
    public Image fillMask;
    public bool offense = true;
    public float fillSmoothing = 0.2f;
    private float currentFill = -1;
    private float targetFill = 1f;
    private float velocity;
    public bool paused;

    private void Awake()
    {
        AutoBattleManager.OnBattleEvent += UpdateDisplay;
        AutoBattleManager.OnStateChange += CheckPause; 
    }
    private void OnDestroy()
    {
        AutoBattleManager.OnBattleEvent -= UpdateDisplay;
        AutoBattleManager.OnStateChange -= CheckPause;
    } 
    private void CheckPause(string id)
    {
        if (id == "battle_start" || id == "battle_enter")
            paused = false;

        else if (id == "battle_exit")
            paused = true;
    }

    private void UpdateDisplay(BattleEvent _)
    {
        Unit unit = offense ? AutoBattleManager.OffenseTeam.parent : AutoBattleManager.DefenseTeam.parent;

        targetFill = unit.GetTotalHealthPercent();
        if (currentFill < 0)
            currentFill = unit.GetTotalHealthPercent(); 

    }

    private void Update()
    {
        if(paused)
        {
            currentFill = -1f;
        }
        else
        {
            if (offense)
                SlideFill(AutoBattleManager.OffenseTeam.parent);
            else
                SlideFill(AutoBattleManager.DefenseTeam.parent);
        } 
    }

    private void SlideFill(Unit unit)
    {
        if (Mathf.Abs(currentFill - targetFill) > 0.001f)
        {
            currentFill = Mathf.SmoothDamp(currentFill, targetFill, ref velocity, fillSmoothing);
        }
        else currentFill = targetFill;

            SetText(unit.team.teamName + " : " + currentFill.ToString("0.0%"));

        fillMask.fillAmount = currentFill;
        if (image.color != unit.team.teamColor)
            SetImageColor(unit.team.teamColor);
    } 
}
