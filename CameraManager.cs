using UnityEngine;
using System.Collections.Generic;

public class CameraManager : MonoBehaviour
{
    private static CameraManager manager;
    public Camera initialCamera;
    public static Camera CurrentCamera { get; private set; }

    [SerializeField] private List<Camera> allCameras = new();

    private void Awake()
    {
        manager = this;
        RefreshCameraList();
    }

    private void Start()
    {
        ChangeCamera(initialCamera);
    }

    private void RefreshCameraList()
    {
        allCameras.Clear();

        Camera[] found = FindObjectsByType<Camera>(FindObjectsSortMode.InstanceID);
        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] != null && allCameras.Contains(found[i]) == false)
                allCameras.Add(found[i]);
        }
    }

    public static void ChangeCamera(Camera cam)
    {
        if (manager == null || cam == null)
            return;

        manager.RefreshCameraList();

        if (manager.allCameras.Contains(cam) == false)
            manager.allCameras.Add(cam);

        for (int i = manager.allCameras.Count - 1; i >= 0; i--)
        {
            if (manager.allCameras[i] == null)
            {
                manager.allCameras.RemoveAt(i);
                continue;
            }

            manager.allCameras[i].enabled = false;
        }

        cam.enabled = true;
        CurrentCamera = cam;
    }
}