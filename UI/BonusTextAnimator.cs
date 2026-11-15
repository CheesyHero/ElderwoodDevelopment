using UnityEngine;
using TMPro;
using System.Collections;

public class BonusTextAnimator : MonoBehaviour
{
    [Header("Text")]
    public TMP_Text textMesh;
    public string message = "BONUS";

    [Header("Timing")]
    public float letterDelay = 0.15f;
    public float blinkDelay = 0.15f;
    public int blinkCount = 3;
    public float restartDelay = 0.5f;

    private Coroutine animationRoutine;

    private void Awake()
    {
        if (!textMesh)
            textMesh = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        animationRoutine = StartCoroutine(AnimateText());
    }

    private void OnDisable()
    {
        if (animationRoutine != null)
            StopCoroutine(animationRoutine);

        if (textMesh)
            textMesh.text = "";
    }

    private IEnumerator AnimateText()
    {
        while (true)
        {
            // Print letters one at a time.
            textMesh.text = "";

            foreach (char letter in message)
            {
                textMesh.text += letter;
                yield return new WaitForSeconds(letterDelay);
            }

            // Blink the completed word three times.
            for (int i = 0; i < blinkCount; i++)
            {
                textMesh.text = "";
                yield return new WaitForSeconds(blinkDelay);

                textMesh.text = message;
                yield return new WaitForSeconds(blinkDelay);
            }

            // Hold before restarting.
            yield return new WaitForSeconds(restartDelay);
        }
    }
}