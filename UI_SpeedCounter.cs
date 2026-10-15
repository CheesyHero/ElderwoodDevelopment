using UnityEngine;

public class UI_SpeedCounter : MonoBehaviour
{
    public int myIndex = 0;

    private void OnEnable()
    {
        Set();
    }

    public void Set()
    {
        gameObject.SetActive(GameSpeedChanger.Index >= myIndex);
    }
}
