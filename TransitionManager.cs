using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public ScreenBlackout blackout;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);

        Debug.Log($"TransitionManager started. TimeScale: {Time.timeScale}");

        if (blackout == null)
        {
            Debug.LogWarning("TransitionManager: No blackout assigned.");
            Transition();
            Complete();
            return;
        }

        blackout.OnCenter = Transition;
        blackout.OnComplete = Complete;

        Debug.Log("TransitionManager: Callbacks registered.");
    }

    public void Transition()
    {
        Debug.Log($"TransitionManager: OnCenter reached. TimeScale: {Time.timeScale}");
        SceneChanger.CompleteChangeRequest();
    }

    public void Complete()
    {
        Debug.Log("TransitionManager: OnComplete reached.");
        Destroy(gameObject);
    }
}