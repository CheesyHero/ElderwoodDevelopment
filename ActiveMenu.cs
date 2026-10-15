using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ActiveMenu : MonoBehaviour
{
    public string UniqueID = "";
    public bool startActive = false;
    public bool destroyOnDisable = false;

    public static List<ActiveMenu> menuStack = new();
    public static List<ActiveMenu> all = new();
    public static bool HasOpenMenu
    { get { return menuStack.Count > 0; } }

    public bool canCancel = true;
    public Transform handle;
    [Tooltip("When this menu is closed, this other menu becomes opened")]
    public ActiveMenu backTo;
     
    public string[] enterIDs;
    public string[] exitIDs;

    public static UnityAction OnMenuOpen;
    public static UnityAction OnMenuClosed;
    public static UnityAction OnAnyUpdate;

    protected virtual void Awake()
    {
        all.Add(this);

        HandleCheck(); 
    }
    protected virtual void OnDestroy()
    {
        all.Remove(this);
    }
    protected virtual void Start()
    {
        if (handle)
            handle.gameObject.SetActive(startActive);
    }

    public virtual void Toggle()
    {
        HandleCheck();

        if (handle)
            handle.gameObject.SetActive(!gameObject.activeInHierarchy);
    }
    public virtual void Open()
    {
        //Debug.Log("ACTIVE MENU: opening " + name);
        HandleCheck();

        if (handle)
            handle.gameObject.SetActive(true);
    }
    public virtual void Set(bool setting)
    {
        //Debug.Log("ACTIVE MENU: opening " + name);
        HandleCheck();

        if (handle)
            handle.gameObject.SetActive(setting);
    }
    public virtual void Close()
    {
        HandleCheck();

        //Debug.Log("ACTIVE MENU: Closing " + name);
        if (handle)
            handle.gameObject.SetActive(false);

        if (backTo)
            backTo.Open();

        foreach(var item in exitIDs)
            OpenByID(item);
    }
    private void HandleCheck()
    { 
        if (!handle)
            handle = transform; 
    }

    protected virtual void OnEnable()
    {
        if (!menuStack.Contains(this))
            menuStack.Add(this);

        OnMenuOpen?.Invoke();
        OnAnyUpdate?.Invoke();

        if (enterIDs != null)
        {
            foreach (var item in enterIDs)
                CloseByID(item);
        }
    }
    protected virtual void OnDisable()
    {
        menuStack.Remove(this);

        OnMenuClosed?.Invoke();
        OnAnyUpdate?.Invoke();

        if (destroyOnDisable)
            Destroy(gameObject);
    }

    public static void CloseLastStack()
    {
        if (menuStack.Count <= 0) return;

        menuStack[^1].Cancel();
    }

    protected virtual void Cancel()
    {
        if (canCancel)
            Close();
    }

    public virtual void BUTTON_CallOpenByID(string ID)
    {
        OpenByID(ID);
    }

    public virtual void BUTTON_CallCloseByID(string ID)
    {
        CloseByID(ID);
    }
    public static void OpenByID(string ID)
    {
        //Debug.Log("OPEN MENU BY ID: Opening " + ID + "..."); 
        foreach(var item in all)
        {
            if (item.UniqueID.Length > 0 && item.UniqueID.ToLower() == ID.ToLower())
                item.Open();
        }
    }
    public static void CloseByID(string ID)
    {
        if (string.IsNullOrEmpty(ID) || menuStack.Count == 0)
            return;

        List<ActiveMenu> toClose = new();

        for (int i = menuStack.Count - 1; i >= 0; i--)
        {
            ActiveMenu item = menuStack[i];

            if (item == null)
                continue;

            if (item.UniqueID.Length > 0 && item.UniqueID.ToLower() == ID.ToLower())
                toClose.Add(item);
        }

        for (int i = 0; i < toClose.Count; i++)
        {
            if (toClose[i] != null)
                toClose[i].Close();
        }
    }
    public static bool HasOpenWindowID(string ID)
    {
        foreach(var item in menuStack)
        {
            if (item.UniqueID.Length > 0 && item.UniqueID == ID)
                return true;
        }

        return false;
    }
}
