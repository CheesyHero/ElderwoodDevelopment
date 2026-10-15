using UnityEngine;
using UnityEngine.Events;

public class ZoneViewerSelector : ZoneViewer
{
    public static UnityAction<object> SelectionOverride;
    public static ZoneViewerSelector manager;
    private void Awake()
    {
        manager = this;
    }
    private void Start()
    {
        CloseWindow();
    }
    private void OnDestroy()
    {
        SelectionOverride = null;
    }
    public void Open(Zone view)
    {
        LoadZone(view);

        gameObject.SetActive(true);
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }


    public static void LoadWindow(Zone view)
    {
        if (!manager || view  == null) 
            CloseWindow(); 

        else manager.Open(view); 
    }
    public static void CloseWindow()
    {
        if (!manager) return;

        manager.Close();
    }
}
