using UnityEngine;

public class UI_LoadPlayerCommands : MonoBehaviour
{
    [SerializeField] ZoneViewer child;
    private void Awake()
    {
        child = GetComponentInChildren<ZoneViewer>();
    }
    private void OnEnable()
    {
        if (child != null)
        {
            GameManager.Player.deck.ResetDeck();
            child.LoadZone(GameManager.Player.deck.deck);
        }
    }
}
