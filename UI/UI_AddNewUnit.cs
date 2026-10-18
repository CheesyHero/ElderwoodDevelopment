using UnityEngine;

public class UI_AddNewUnit : MonoBehaviour
{
    private void Awake()
    {
        UI_FullArmyDisplay.OnUpdateUnit += Refresh;
        UI_FullArmyDisplay.OnUpdateHero += Refresh;
    }
    private void OnDestroy()
    {
        UI_FullArmyDisplay.OnUpdateUnit -= Refresh;
        UI_FullArmyDisplay.OnUpdateHero -= Refresh;
    }

    private void Refresh(object _) => gameObject.SetActive(GameManager.Player.units.Count < 8); 
}
