using UnityEngine;

public class OpenMenuPrefab : MonoBehaviour
{ 
    public ActiveMenu prefab;
    ActiveMenu child;
    public void OpenPrefab()
    {
        if (prefab == null)
            return;

        if (!child) child = Instantiate(prefab, transform.position, Quaternion.identity);
        else child.gameObject.SetActive(true);
    }
    public void ClosePrefab()
    {
        if (!child) return;

        child.gameObject.SetActive(false);
    }
}
