using Unity.VisualScripting.FullSerializer;
using UnityEngine;

public class BannerArray : MonoBehaviour
{
    public static BannerArray manager;
    [SerializeField] BannerManager[] banners;

    private void Awake()
    {
        manager = this;
        banners = GetComponentsInChildren<BannerManager>();
    }

    public static void PrintReport(string report)
    {
        if (!manager) return; 
        manager.Print(report);
    }
    public void Print(string report)
    {
        var get = GetNextAvailable();
        get.transform.SetAsLastSibling();
        get.PrintReport(report);
    }

    public BannerManager GetNextAvailable()
    {
        if (banners.Length <= 0) return null;

        foreach (var item in banners)
        {
            if (!item.IsPrinting)
                return item;
        }

        return banners[0];
    }
}
