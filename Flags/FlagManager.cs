using System.Collections.Generic;
using UnityEngine;

public enum FlagType
{
    temporary = 0,
    permanant = 1,
    session = 2
}

public static class FlagManager
{
    private readonly static Dictionary<string, FlagType> flags = new();

    public static void AddFlag(string name, FlagType type)
    {
        bool exists = flags.ContainsKey(name);
        flags[name] = type;

        Debug.Log($"[Flags] {(exists ? "Updated" : "Added")}: {name} ({type})");
    }

    public static void RemoveFlag(string name)
    {
        if (flags.Remove(name))
            Debug.Log($"[Flags] Removed: {name}");
        else
            Debug.LogWarning($"[Flags] Cannot remove: {name} (Not Found)");
    }

    public static bool ContainsFlag(string name)
    {
        bool exists = flags.ContainsKey(name);

        Debug.Log($"[Flags] Validating: {name} - {(exists ? "TRUE" : "FALSE")}");

        return exists;
    }

    public static void ClearTemporaryFlags()
    {
        List<string> toRemove = new();

        foreach (var flag in flags)
        {
            if (flag.Value == FlagType.temporary)
                toRemove.Add(flag.Key);
        }

        foreach (string name in toRemove)
            RemoveFlag(name);
    }

    public static string[] ExportPermanantFlags()
    {
        List<string> exported = new();

        foreach (var flag in flags)
        {
            if (flag.Value == FlagType.permanant)
                exported.Add(flag.Key);
        }

        return exported.ToArray();
    }

    public static void LoadPermanantFlags(string[] savedFlags)
    {
        if (savedFlags == null)
            return;

        foreach (string name in savedFlags)
            AddFlag(name, FlagType.permanant);
    }

    public static void DebugCommand()
    {
        AddFlag("First Game", FlagType.permanant); // used in rewards manager
    }
}