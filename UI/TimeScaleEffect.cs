using UnityEngine;
using System.Collections;

public class TimeScaleEffect : MonoBehaviour
{
    private static TimeScaleEffect instance;

    [SerializeField]
    private AnimationCurve recoveryCurve =
        AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine activeEffect;

    private void Awake()
    {
        instance = this;
    }

    public static void TriggerSlowMotion(float slowTimeScale = 0.1f, float holdDuration = 0.1f, float recoveryDuration = 0.5f)
    { 
        if (instance == null)
            return;

        if (instance.activeEffect != null)
            return;

        instance.activeEffect = instance.StartCoroutine(instance.SlowMotionRoutine(slowTimeScale, holdDuration, recoveryDuration));
    }

    private IEnumerator SlowMotionRoutine(float slowTimeScale, float holdDuration, float recoveryDuration)
    {
        Time.timeScale = slowTimeScale * GameSpeedChanger.currentSpeed;

        yield return new WaitForSecondsRealtime(holdDuration);

        float elapsed = 0f;

        while (elapsed < recoveryDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / recoveryDuration);
            float curveValue = recoveryCurve.Evaluate(t);

            Time.timeScale = Mathf.Lerp(
                slowTimeScale, GameSpeedChanger.currentSpeed, curveValue );

            yield return null;
        }

        Time.timeScale = GameSpeedChanger.currentSpeed;
        activeEffect = null;
    }

    private void OnDisable()
    {
        if (activeEffect != null)
            StopCoroutine(activeEffect);

        activeEffect = null;
        Time.timeScale = 1f;

        if (instance == this)
            instance = null;
    }
}