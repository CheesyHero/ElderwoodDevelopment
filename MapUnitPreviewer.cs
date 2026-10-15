using Unity.VisualScripting;
using UnityEngine;

public class MapUnitPreviewer : MonoBehaviour
{
    public static MapUnitPreviewer manager;

    [SerializeField]
    private Selectable current;

    [SerializeField]
    private UI_UnitPreviewLoader loader;

    private void Awake()
    {
        manager = this;

        PlayerSelection.OnNewHighlight += LoadPreview;
    } 

    private void OnDestroy()
    {
        if (manager == this)
            manager = null;

        PlayerSelection.OnNewHighlight -= LoadPreview;
    }

    private void Load(Unit unit)
    {
        if (!loader) return;
         
        if (unit != null)
        {
            loader.SetActive(true);
            loader.LoadUnit(unit);
        }
        else
        {
            loader.SetActive(false);
        }
    }
    private void Load(Selectable obj)
    {
        if (!loader) return;

        current = obj;

        if (current is HexWalker walker && walker.PlayerVisible())
        {
            loader.SetActive(true);
            loader.LoadUnit(walker.Parent);
        }
        else
        {
            loader.SetActive(false);
        }
    }

    private void Close()
    {
        current = null; 

        if (loader) 
            loader.SetActive(false);
    }

    public static void LoadPreview(Unit u)
    {
        if (!manager) return;

        manager.Load(u);
    }

    public static void LoadPreview(Selectable obj)
    {
        if (!manager) return;

        manager.Load(obj);
    }

    public static void ClosePreview()
    {
        if (!manager) return;

        manager.Close();
    }
}