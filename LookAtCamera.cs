using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private void Update()
    {
        if (CameraManager.CurrentCamera != null)
            transform.rotation = CameraManager.CurrentCamera.transform.rotation;
    }
}
