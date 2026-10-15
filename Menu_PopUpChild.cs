using UnityEngine;

public class Menu_PopUpChild : MonoBehaviour
{
    public string NameID = "New Menu";

    private void Start()
    {
        Close();
    }

    public void Open(Vector2 location)
    {
        gameObject.transform.position = location;
        gameObject.SetActive(true);
    }
    public void Close()
    {
        gameObject.SetActive(false);
    }
}
