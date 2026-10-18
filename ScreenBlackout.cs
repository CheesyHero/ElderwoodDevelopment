using UnityEngine;
using UnityEngine.Events;

public class ScreenBlackout : MonoBehaviour
{
    public float speed = 3;

    public UnityAction OnCenter;
    public UnityAction OnComplete;

    private void Awake()
    {
        transform.position = new Vector2(-Screen.width, Screen.height / 2);
    }
    private void Start()
    {
        LeanTween.move(gameObject, new Vector2(Screen.width / 2, Screen.height / 2), speed).setOnComplete(CleanUp).setEaseOutQuad();
        Debug.Log($"{name} : Moving to center {transform.position} -> {new Vector2(Screen.width / 2, Screen.height / 2)}");
    } 

    private void CleanUp()
    {
        OnCenter?.Invoke();

        LeanTween.move(gameObject, new Vector2(Screen.width * 2, Screen.height / 2), speed).setOnComplete(Complete).setEaseInQuad();
        Debug.Log($"{name} : Moving to exit {transform.position} -> {new Vector2(Screen.width * 2, Screen.height / 2)}");
    }
    private void Complete()
    {
        Debug.Log("Complete");
        OnComplete?.Invoke();
    }
}
