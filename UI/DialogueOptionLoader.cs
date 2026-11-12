using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueOptionLoader : MonoBehaviour
{
    public Button buttonA, buttonB, skipB;
    public TextMeshProUGUI textA, textB;

    public void LoadDialoguePacket(DialoguePacket input)
    {
        if (!buttonA || !buttonB || !textA || !textB)
            return;

        bool hasOptionA = !string.IsNullOrWhiteSpace(input.optionA);
        bool hasOptionB = !string.IsNullOrWhiteSpace(input.optionB);

        buttonA.gameObject.SetActive(hasOptionA);
        buttonB.gameObject.SetActive(hasOptionB);
        skipB.gameObject.SetActive(!hasOptionB);

        textA.text = hasOptionA ? input.optionA : "";
        textB.text = hasOptionB ? input.optionB : "";
    }
}