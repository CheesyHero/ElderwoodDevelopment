using System.Collections.Generic;
using UnityEngine;

public class Menu_PopUpCaller : MonoBehaviour
{ 
    public static Menu_PopUpCaller instance;
    public List<Menu_PopUpChild> children;

    private void Awake()
    {
        instance = this;
        children = new List<Menu_PopUpChild>(FindObjectsByType<Menu_PopUpChild>(FindObjectsSortMode.None)); 
    }

    public static void OpenMenu(string menuName, Vector2 screenPos)
    {
        foreach (var item in instance.children)
            if (item.NameID == menuName)
                item.Open(screenPos);

            else item.Close();
    }
    public static void CloseAll()
    {
        foreach (var item in instance.children)
                item.Close();
    }
}
