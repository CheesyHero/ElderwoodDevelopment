using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialoguePrinter : MonoBehaviour
{
    [Header("Dialogue")]
    public TextMeshProUGUI dialogueText;
    public DialogueOptionLoader optionLoader;

    private DialoguePacket currentPacket;
    private bool hasDialogue;

    [Header("Typewriter")]
    public float characterDelay = 0.03f;
    public AudioSource audioSource;
    public Vector2 pitchRange = new Vector2(0.8f, 1.2f);
    public int soundFrequency = 3;

    [Header("Actor Animation")]
    public Animator animator; 
    private Coroutine routine;

    [Header("Raycast Blocking")]
    public CanvasGroupFader backgroundFader;

    public bool IsPrinting => routine != null;
    public DialoguePacket CurrentPacket => currentPacket;

    public static DialoguePrinter Instance { get; private set; }

    private void Awake()
    {
        //if (Instance != null && Instance != this)
        //{
        //   Debug.LogWarning("Duplicate DialoguePrinter detected.");
        //    Destroy(gameObject);
        //    return;
        //}

        Instance = this;

        // Initialize dialogue state.
        hasDialogue = false;
        currentPacket = default;
        routine = null;

        SetAnimatorBool("talking", false);
        SetAnimatorBool("active", false);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public static void StartDialogue(string title)
    {
        if (Instance == null)
        {
            Debug.LogWarning("No DialoguePrinter instance found.");
            return;
        }

        Instance.PrintText(title);
    } 

    // Load dialogue directly from the database.
    public void PrintText(string title) => PrintText(DialogueDatabase.PullConversation(title));

    // Print an existing dialogue packet.
    public void PrintText(DialoguePacket packet)
    {
        if (string.IsNullOrEmpty(packet.title))
        {
            EndDialogue();
            return;
        }

        // Only fade in when starting a NEW conversation.
        if (!hasDialogue)
        {
            ActiveMenu.OpenByID("dialogue");
            SetBackground(true);
        }

        currentPacket = packet;
        hasDialogue = true;

        SetAnimatorBool("active", true);
        StartPrint();
    }

    // Pass the current packet to the option loader.
    public void LoadOptions()
    {
        if (optionLoader == null)
            return;

        optionLoader.LoadDialoguePacket(currentPacket);
    }

    private void StartPrint()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        SetAnimatorBool("talking", false);
        StopAudio();

        if (!hasDialogue || dialogueText == null)
            return;

        LoadOptions();

        routine = StartCoroutine(TextPrinterRoutine());
    }

    private IEnumerator TextPrinterRoutine()
    {
        SetAnimatorBool("talking", true);

        dialogueText.text = currentPacket.content ?? "";
        dialogueText.maxVisibleCharacters = 0;
        dialogueText.ForceMeshUpdate();

        int totalCharacters =
            dialogueText.textInfo.characterCount;

        float timer = 0f;
        int lastSoundCharacter = 0;

        while (dialogueText.maxVisibleCharacters < totalCharacters)
        {
            timer += Time.unscaledDeltaTime;

            int charactersToReveal = characterDelay <= 0f
                ? totalCharacters
                : Mathf.FloorToInt(timer / characterDelay);

            if (charactersToReveal > 0)
            {
                dialogueText.maxVisibleCharacters = Mathf.Min(
                    dialogueText.maxVisibleCharacters + charactersToReveal,
                    totalCharacters
                );

                if (characterDelay > 0f)
                    timer -= charactersToReveal * characterDelay;

                if (dialogueText.maxVisibleCharacters - lastSoundCharacter
                    >= Mathf.Max(1, soundFrequency))
                {
                    PlayAudio();
                    lastSoundCharacter =
                        dialogueText.maxVisibleCharacters;
                }
            }

            yield return null;
        }

        dialogueText.maxVisibleCharacters = totalCharacters;

        routine = null;

        StopAudio();
        SetAnimatorBool("talking", false);
    }

    public void PlayAudio()
    {
        if (audioSource == null || audioSource.clip == null)
            return;

        audioSource.Stop();

        audioSource.pitch = Random.Range(
            Mathf.Min(pitchRange.x, pitchRange.y),
            Mathf.Max(pitchRange.x, pitchRange.y)
        );

        audioSource.Play();
    }

    private void StopAudio()
    {
        if (audioSource != null)
            audioSource.Stop();
    }

    public void SelectOptionA()
    {
        if (!hasDialogue)
            return;

        if (IsPrinting)
        {
            SkipToEnd();
            return;
        }

        SaveFlag();

        if (!string.IsNullOrWhiteSpace(currentPacket.pointerA))
            PrintText(currentPacket.pointerA);

        else EndDialogue();
    }

    public void SelectOptionB()
    {
        if (!hasDialogue)
            return;

        if (IsPrinting)
        {
            SkipToEnd();
            return;
        }

        SaveFlag();

        if (!string.IsNullOrWhiteSpace(currentPacket.pointerB))
            PrintText(currentPacket.pointerB);

        else EndDialogue();
    }

    public void SkipToEnd()
    {
        if (!IsPrinting || dialogueText == null)
            return;

        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        dialogueText.maxVisibleCharacters = int.MaxValue;

        StopAudio();
        SetAnimatorBool("talking", false);
    }

    public void EndDialogue()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        hasDialogue = false;
        currentPacket = default;

        StopAudio();

        SetAnimatorBool("talking", false);
        SetAnimatorBool("active", false);

        if (dialogueText != null)
            dialogueText.text = "";

        // Hide both option buttons.
        LoadOptions(); SetBackground(false);
        ActiveMenu.CloseByID("dialogue");
    }

    public void SetAnimatorBool(string parameter, bool value)
    {
        if (animator == null)
            return;

        animator.SetBool(parameter, value);
    }

    private void OnDisable()
    {
        if (routine != null)
        {
            StopCoroutine(routine);
            routine = null;
        }

        hasDialogue = false;
        currentPacket = default;

        StopAudio();

        SetAnimatorBool("talking", false);
        SetAnimatorBool("active", false);
    }

    public void SaveFlag()
    {
        if (string.IsNullOrWhiteSpace(currentPacket.completeFlag) ||
            string.IsNullOrWhiteSpace(currentPacket.completeFlag) ||
            string.IsNullOrWhiteSpace(currentPacket.flagType) ||
            currentPacket.flagType == "none") return;

        var type = currentPacket.flagType switch
        {
            "session" => FlagType.session,
            "temporary" => FlagType.temporary,
            _ => FlagType.permanant,
        };

        FlagManager.AddFlag(currentPacket.completeFlag, type);
    }

    public void SetBackground(bool fadeIn)
    {
        Debug.Log(
            $"[Dialogue] SetBackground({fadeIn}) called.\n" +
            System.Environment.StackTrace
        );

        backgroundFader?.Fade(fadeIn);
    }
    public void SkipAll()
    {
        if (!hasDialogue)
            return;

        // Prevent infinite loops caused by circular pointers.
        HashSet<string> visited = new HashSet<string>();

        while (hasDialogue)
        {
            string title = currentPacket.title;

            if (!visited.Add(title))
            {
                Debug.LogWarning(
                    $"[Dialogue] SkipAll detected circular dialogue: {title}");
                break;
            }

            bool hasOptionA =
                !string.IsNullOrWhiteSpace(currentPacket.optionA);

            bool hasOptionB =
                !string.IsNullOrWhiteSpace(currentPacket.optionB);

            // Stop when both options are empty or both are populated.
            if (hasOptionA == hasOptionB)
                break;

            // Only advance automatically through Option A.
            // Never choose Option B automatically.
            if (!hasOptionA)
                break;

            string pointer = currentPacket.pointerA;

            // Match the normal option selection behavior.
            SaveFlag();

            if (string.IsNullOrWhiteSpace(pointer))
            {
                EndDialogue();
                return;
            }

            DialoguePacket nextPacket =
                DialogueDatabase.PullConversation(pointer);

            if (string.IsNullOrWhiteSpace(nextPacket.title))
            {
                EndDialogue();
                return;
            }

            PrintText(nextPacket);
        }

        // Reveal the final packet immediately.
        if (hasDialogue && IsPrinting)
            SkipToEnd();
    }
}
