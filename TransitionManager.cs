using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    public ScreenBlackout blackout;
    public delegate void OnComplete();

    private void Start()
    {
        if (blackout == null)
        {
            Transition();
            Complete();
        }

        blackout.OnCenter = Transition;
        blackout.OnComplete = Complete;

        DontDestroyOnLoad(gameObject);
    }
    public void Transition()
    {
        SceneChanger.CompleteChangeRequest();
    }
    public void Complete()
    {
        Destroy(gameObject);
    }
}
