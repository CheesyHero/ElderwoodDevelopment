using System;
using UnityEngine;

public class GameSpeedChanger : MonoBehaviour
{
    public static int Index { get { return i; } }
    static int i = 0;
    static readonly float[] speedOptions = new float[] { 1, 2, 3 };

    private void OnEnable()
    {
        ReturnToLastIndexSpeed();
    }
    private void OnDisable()
    {
        ReturnToOriginalSpeed();
    }
    private void OnDestroy()
    {
        ReturnToOriginalSpeed();
    }

    public static void ChangeGameSpeed()
    {
        i++;

        if (i >= speedOptions.Length)
            i = 0;

        ReturnToLastIndexSpeed();
    }
    public static void ReturnToLastIndexSpeed()
    {
        Time.timeScale = speedOptions[i];
    }
    public static void ReturnToOriginalSpeed()
    {
        Time.timeScale = speedOptions[0];
    }
}
