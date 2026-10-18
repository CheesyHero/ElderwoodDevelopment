using UnityEngine;
using TMPro;

public class UI_TurnTimer : MonoBehaviour
{
    public TextMeshProUGUI textMesh;

    private void Awake()
    {
        TurnManager.OnEventTrigger += UpdateText;
    }
    private void Start()
    {
        UpdateText();
    }
    private void OnDestroy()
    {
        TurnManager.OnEventTrigger -= UpdateText;
    }

    private void UpdateText(string _) => UpdateText();
    public void UpdateText()
    {
        if (!textMesh) return;
         
        textMesh.text = $"{HexCrawlManager.TurnsRemaining} Turns Remaining";
    }
}
