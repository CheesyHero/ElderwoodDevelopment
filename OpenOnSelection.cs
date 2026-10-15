using UnityEngine;

public class OpenOnSelection : MonoBehaviour
{
    public bool initial = false;
    public bool invert = false;
    public GameObject handle;
    private void Awake()
    {
        if (!handle) handle = gameObject;
        UI_FullArmyDisplay.OnUpdateHero += Open;
        UI_FullArmyDisplay.OnUpdateUnit += Open;
    }
    private void Start()
    {
        handle.SetActive(initial);
    }
    private void OnDestroy()
    {
        UI_FullArmyDisplay.OnUpdateHero -= Open;
        UI_FullArmyDisplay.OnUpdateUnit -= Open;
    }
    public void Open()
    {
        handle.SetActive(true && !invert);
    }
    public void Close()
    {
        handle.SetActive(false && !invert);
    }
    public void Open(Hero _) => Open();
    public void Open(Unit _) => Open();
}
