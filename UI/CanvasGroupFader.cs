
using System;
using System.Collections;
using UnityEngine;

public class CanvasGroupFader : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float fadeDuration = 0.25f;

    private Coroutine fadeRoutine;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Fade(bool fadeIn, Action onComplete = null)
    {
        if (canvasGroup == null)
            return;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        canvasGroup.blocksRaycasts = fadeIn;
        canvasGroup.interactable = fadeIn;

        fadeRoutine = StartCoroutine(FadeRoutine(fadeIn ? 1f : 0f, onComplete ));
    }

    private IEnumerator FadeRoutine(
        float targetAlpha,
        Action onComplete)
    {
        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = fadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / fadeDuration);

            canvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                t
            );

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        fadeRoutine = null;

        onComplete?.Invoke();
    }

    public void SetImmediate(bool visible)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (canvasGroup == null)
            return;

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.blocksRaycasts = visible;
        canvasGroup.interactable = visible;
    }
}
