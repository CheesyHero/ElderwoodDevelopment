using UnityEngine;

public class StartGameButton : MonoBehaviour
{
    public World startWorld;
    public ArmyGenerator playerArmyReference;

    public void StartGame() =>
        GameManager.NewGame(playerArmyReference, startWorld); 
}
