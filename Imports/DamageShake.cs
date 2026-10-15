
using System.Collections; 
using UnityEngine;

public class DamageShake : MonoBehaviour
{
    public BattleCombatant parent;
    public Transform handle;
    public float weight = 0.5f;
    public float duration = 0.25f;

    private void Awake()
    {
        AutoBattleManager.OnBattleEvent += Check;
    }
    private void Start()
    { 
        origin = handle.localPosition;
    }

    private void OnDestroy()
    {
        AutoBattleManager.OnBattleEvent -= Check;
    }

    private void Check(BattleEvent info)
    {
        if (info.eventType == Enum_BattleEvent_Type.onDamageTaken && info.owner == parent)
            Shake(info.value);
    }

    public void Shake(float damage)
    {
        if(coroutine != null)
            StopCoroutine(coroutine);

        float mult = Mathf.Clamp(damage / parent.hero.health.health_max, 0.1f, 1f);

        coroutine = StartCoroutine(ShakeRoutine(mult, mult));
    }

    Coroutine coroutine;
    Vector2 origin;

    IEnumerator ShakeRoutine(float multA, float multB)
    {
        float time = 0;

        while(time < duration * multB)
        {
            handle.transform.localPosition = origin + GetShakePos(multA);

            time += Time.deltaTime;
            yield return null;
        }

        handle.transform.localPosition = origin;
        coroutine = null;
    }
    Vector2 GetShakePos(float mult)
    {
        float x = Random.Range(-weight, weight);
        float y = Random.Range(-weight, weight);

        x = Mathf.Clamp(x, -1, 1);
        y = Mathf.Clamp(y, -1, 1);

        return new Vector2(x, y) * mult;
    }
}
