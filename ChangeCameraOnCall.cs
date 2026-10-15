using System.Collections.Generic;
using UnityEngine;

public class ChangeCameraOnCall : MonoBehaviour
{
    public Camera cam;
    public List<string> enable = new();

    private void OnEnable()
    {
        AutoBattleManager.OnStateChange += CheckState;
    }

    private void OnDisable()
    {
        AutoBattleManager.OnStateChange -= CheckState;
    }

    public void CheckState(string state)
    {
        if (cam == null)
            return;

        if (enable.Contains(state))
            CameraManager.ChangeCamera(cam);
    }
}