
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public struct DialoguePacket
{
    public string title;

    // Conditional dialogue
    public string skipFlag;
    public string skipPointer;
    public string requiredFlag;
    public string failPointer;

    // Commands
    public string commandStart;

    // Actor and content
    public string actor;
    public bool leftPosition;
    public string content;

    // Dialogue options
    public string optionA, pointerA, commandEndA;
    public string optionB, pointerB, commandEndB;
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

        // Prevent circular dialogue redirects.
        HashSet<string> visited = new HashSet<string>();

        return ResolveDialogue(title, visited);
    }

    private static DialoguePacket ResolveDialogue(
        string title,
        HashSet<string> visited)
    {
        if (string.IsNullOrWhiteSpace(title))
            return default;

        title = title.Trim().ToLowerInvariant();

        // Detect circular redirects.
        if (!visited.Add(title))
        {
            Debug.LogWarning(
                $"[Dialogue] Circular redirect detected: {title}");

            return default;
        }

        if (!database.TryGetValue(title, out DialoguePacket packet))
        {
            Debug.LogWarning($"[Dialogue] Not found: {title}");
            return default;
        }

        // SKIP FLAG
        // If the flag exists, this dialogue is locked out.
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
        // The dialogue can only play if this flag exists.
        if (!string.IsNullOrWhiteSpace(packet.requiredFlag))
        {
            if (!FlagManager.ContainsFlag(packet.requiredFlag))
            {
                if (!string.IsNullOrWhiteSpace(packet.failPointer))
                    return ResolveDialogue(packet.failPointer, visited);

                return default;
            }
        }

        // All conditions passed.
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

        // Skip header row.
        for (int i = 1; i < rows.Count; i++)
        {
            List<string> row = rows[i];

            if (row.Count < 15)
            {
                Debug.LogWarning(
                    $"Dialogue row {i + 1} has fewer than 15 columns.");
                continue;
            }

            string title = row[0].Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(title))
                continue;

            DialoguePacket packet = new DialoguePacket
            {
                title = title,

                // Conditional dialogue
                skipFlag = row[1].Trim().ToLowerInvariant(),
                skipPointer = row[2].Trim().ToLowerInvariant(),
                requiredFlag = row[3].Trim().ToLowerInvariant(),
                failPointer = row[4].Trim().ToLowerInvariant(),

                // Commands
                commandStart = row[5].Trim(),

                // Actor and content
                actor = row[6].Trim().ToLowerInvariant(),
                leftPosition = row[7].Trim().Equals(
                    "left", StringComparison.OrdinalIgnoreCase),
                content = row[8],

                // Option A
                optionA = row[9],
                pointerA = row[10].Trim().ToLowerInvariant(),
                commandEndA = row[11].Trim(),

                // Option B
                optionB = row[12],
                pointerB = row[13].Trim().ToLowerInvariant(),
                commandEndB = row[14].Trim()
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
                if (inQuotes && i + 1 < csv.Length &&
                    csv[i + 1] == '"')
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
                if (c == '\r' && i + 1 < csv.Length &&
                    csv[i + 1] == '\n')
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
