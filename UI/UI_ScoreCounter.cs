using UnityEngine;

public class UI_ScoreCounter : ComponentLoader
{
    public float currentValue;
    public bool offense = true;
    public float speed = 10f;
    public float minSpeed = 10f;

    private void Update()
    {
        BattleUnit unit = offense ? AutoBattleManager.OffenseTeam : AutoBattleManager.DefenseTeam;
        if (unit == null)
            return;

        float target = unit.workPoints;

        if (Mathf.Abs(currentValue - target) > 0.01f)
            Fiddle(target);
        else
            currentValue = target;

        SetText(Mathf.RoundToInt(currentValue).ToString());
    }

    void Fiddle(float target)
    {
        float distance = Mathf.Abs(target - currentValue);
        float step = Mathf.Max(distance * speed, minSpeed) * Time.deltaTime;

        if (currentValue < target)
            currentValue = Mathf.Min(currentValue + step, target);
        else
            currentValue = Mathf.Max(currentValue - step, target);
    }
}