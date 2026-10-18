using UnityEngine;

public class UI_EndGame : MonoBehaviour
{ 
    public GameObject victoryHandle;
    public GameObject defeatHandle;

    private void OnEnable()
    {
        LoadResult();
    }
    public void LoadResult()
    {
        victoryHandle.SetActive(GameManager.Player == HexCrawlManager.Winner);
        defeatHandle.SetActive(GameManager.Player != HexCrawlManager.Winner);
    }
}
