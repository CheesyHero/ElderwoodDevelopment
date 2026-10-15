using System.Collections;
using UnityEngine;

public class TestBattle : MonoBehaviour
{
    public float wait;
    public HexTile terrain;
    public Unit teamA;
    public Unit teamB;

    private void Awake()
    {
        HexWalker.OverrideTerritory(terrain);

        teamA.Initialize(true);
        teamA.Refresh(1, true);
        teamB.Initialize(true);
        teamB.Refresh(1, true);
    }
    void Start()
    {
        StartCoroutine(StartBattleIn(wait));
    }
    IEnumerator StartBattleIn(float dur)
    {
        yield return new WaitForSeconds(dur);
        AutoBattleManager.InitiateBattle(teamA, teamB);
    }
}
