
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public struct DialoguePacket
{
    public string title;

    // Actor and content
    public string actor;
    public bool leftPosition;
    public string content;

    // Dialogue options
    public string optionA, pointerA, commandEndA;
    public string optionB, pointerB, commandEndB;

    // Completion flags
    public string completeFlag;
    public string flagType;

    // Conditional dialogue
    public string skipFlag;
    public string skipPointer;
    public string requiredFlag;
    public string failPointer;

    // Commands
    public string commandStart;
}

public static class DialogueDatabase
{
    private static Dictionary<string, DialoguePacket> database;

    private const string csvPath = "Dialogue/dialogue";

    public static DialoguePacket PullConversation(string title)
    {
        if (database == null)
            LoadDatabase();

        if (string.IsNullOrWhiteSpace(title))
            return default;

        title = title.Trim().ToLowerInvariant();

        HashSet<string> visited = new HashSet<string>();
        return ResolveDialogue(title, visited);
    }

    private static DialoguePacket ResolveDialogue(string title, HashSet<string> visited)
    {
        if (string.IsNullOrWhiteSpace(title))
            return default;

        title = title.Trim().ToLowerInvariant();

        if (!visited.Add(title))
        {
            Debug.LogWarning($"[Dialogue] Circular redirect detected: {title}");
            return default;
        }

        if (!database.TryGetValue(title, out DialoguePacket packet))
        {
            Debug.LogWarning($"[Dialogue] Not found: {title}");
            return default;
        }

        // SKIP FLAG
        // Redirect if the skip flag already exists.
        if (!string.IsNullOrWhiteSpace(packet.skipFlag))
        {
            if (FlagManager.ContainsFlag(packet.skipFlag))
            {
                if (!string.IsNullOrWhiteSpace(packet.skipPointer))
                    return ResolveDialogue(packet.skipPointer, visited);

                return default;
            }
        }

        // REQUIRED FLAG
        // Redirect if the required flag doesn't exist.
        if (!string.IsNullOrWhiteSpace(packet.requiredFlag))
        {
            if (!FlagManager.ContainsFlag(packet.requiredFlag))
            {
                if (!string.IsNullOrWhiteSpace(packet.failPointer))
                    return ResolveDialogue(packet.failPointer, visited);

                return default;
            }
        }

        return packet;
    }

    public static void LoadDatabase()
    {
        database = new Dictionary<string, DialoguePacket>();

        TextAsset csv = Resources.Load<TextAsset>(csvPath);

        if (csv == null)
        {
            Debug.LogError($"Dialogue CSV not found: {csvPath}");
            return;
        }

        List<List<string>> rows = ParseCSV(csv.text);

        for (int i = 1; i < rows.Count; i++)
        {
            List<string> row = rows[i];

            if (row.Count < 17)
            {
                Debug.LogWarning($"Dialogue row {i + 1} has fewer than 17 columns.");
                continue;
            }

            string title = row[0].Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(title))
                continue;

            DialoguePacket packet = new DialoguePacket
            {
                title = title,

                // Actor and content
                actor = row[1].Trim().ToLowerInvariant(),
                leftPosition = row[2].Trim().Equals("left", StringComparison.OrdinalIgnoreCase),
                content = row[3],

                // Option A
                optionA = row[4],
                pointerA = row[5].Trim().ToLowerInvariant(),

                // Option B
                optionB = row[6],
                pointerB = row[7].Trim().ToLowerInvariant(),

                // Completion flags
                completeFlag = row[8].Trim(),
                flagType = row[9].Trim().ToLowerInvariant(),

                // Conditional dialogue
                skipFlag = row[10].Trim(),
                skipPointer = row[11].Trim().ToLowerInvariant(),
                requiredFlag = row[12].Trim(),
                failPointer = row[13].Trim().ToLowerInvariant(),

                // Commands
                commandStart = row[14].Trim(),
                commandEndA = row[15].Trim(),
                commandEndB = row[16].Trim()
            };

            if (database.ContainsKey(title))
            {
                Debug.LogWarning($"Duplicate dialogue title: {title}");
                continue;
            }

            database.Add(title, packet);
        }

        Debug.Log($"Loaded {database.Count} dialogue packets.");
    }

    // Save the completion flag associated with a dialogue packet.
    public static void SaveCompletionFlag(DialoguePacket packet)
    {
        if (string.IsNullOrWhiteSpace(packet.completeFlag))
            return;

        if (string.IsNullOrWhiteSpace(packet.flagType) || packet.flagType == "none")
            return;

        if (!Enum.TryParse(packet.flagType, true, out FlagType type) ||
            !Enum.IsDefined(typeof(FlagType), type))
        {
            Debug.LogWarning($"[Dialogue] Invalid flag type: {packet.flagType}");
            return;
        }

        FlagManager.AddFlag(packet.completeFlag, type);
    }

    private static List<List<string>> ParseCSV(string csv)
    {
        List<List<string>> rows = new List<List<string>>();
        List<string> row = new List<string>();
        StringBuilder field = new StringBuilder();

        bool inQuotes = false;

        for (int i = 0; i < csv.Length; i++)
        {
            char c = csv[i];

            if (c == '"')
            {
                if (inQuotes && i + 1 < csv.Length && csv[i + 1] == '"')
                {
                    field.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                row.Add(field.ToString());
                field.Clear();
            }
            else if ((c == '\n' || c == '\r') && !inQuotes)
            {
                if (c == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                    i++;

                row.Add(field.ToString());
                field.Clear();

                rows.Add(row);
                row = new List<string>();
            }
            else
            {
                field.Append(c);
            }
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        return rows;
    }
}
