using UnityEditor;
using UnityEngine;
public enum PrefixType
{
    Generic,
    Burrowguard,
    Treefolk,
    Academy
}

public enum SuffixType
{
    Generic,
    Nature,
    Combat,
    Noble,
    Mystic,
    Furfolk,
    Featherfolk,
    Scalefolk
}
public static class RandomNameGenerator
{
    // --------------------
    // Prefix Pools
    // --------------------
    private static readonly string[] genericPrefixes =
    {
        "stone","dirt","wood","run","tree","earth","mud"
    };

    private static readonly string[] burrowguardPrefixes =
    {
        "stone","deep","iron","gold","mud","tunnel",
        "hammer","steel","dig", "dirt","sparkle"
    };

    private static readonly string[] treefolkPrefixes =
    {
        "green","tangle","bark","tree","wind","thief","band","shadow","penny","shiny"
    };

    private static readonly string[] academyPrefixes =
    {
        "book","wand","spell","hex","whisper","page","letter","pen","ink"
    };

    private static readonly string[] furfolkSuffixes =
    {
    "paw","fang","claw","tail",
    "fur","mane","tooth","bite"
    };

    private static readonly string[] featherfolkSuffixes =
    {
    "wing","claw","talon","crest","beak",
    "quill","down","wind","feather", 
    };

    private static readonly string[] scalefolkSuffixes =
    {
    "scale","fang","hide","crest",
    "spike","fang","bite","hiss","tail","tooth"
    };

    private static readonly string[] genericSuffixes =
    {
    "tail","foot","paw","eye","ear",
    "kin","step","hand","mark","song",
    "shade","crest","heart","path","watch",
    "spark","flare","shade","glow","trace"
    };

    private static readonly string[] natureSuffixes =
    {
        "leaf","tree","ling","root","brush"
    };

    private static readonly string[] combatSuffixes =
    {
        "sword","shield","boot","spear","hammer","claw","tooth"
    };

    private static readonly string[] nobleSuffixes =
    {
    "king","heart","queen","sword","rad","ward"
    };

    private static readonly string[] mysticSuffixes =
    {
    "star","eye","glyph","moon","sun"
    };

    // --------------------
    // Public Generator
    // --------------------

    public static string GenerateName(PrefixType prefixType, SuffixType suffixType)
    {
        string prefix = GetRandomFromPool(GetPrefixPool(prefixType));
        string suffix = GetRandomFromPool(GetSuffixPool(suffixType));

        return Capitalize(prefix) + suffix;
    }

    // --------------------
    // Pool Selection
    // --------------------

    private static string[] GetPrefixPool(PrefixType type)
    {
        return type switch
        {
            PrefixType.Burrowguard => burrowguardPrefixes,
            PrefixType.Treefolk => treefolkPrefixes,
            PrefixType.Academy => academyPrefixes, 
            _ => genericPrefixes
        };
    }

    private static string[] GetSuffixPool(SuffixType type)
    {
        return type switch
        {
            SuffixType.Nature => natureSuffixes,
            SuffixType.Combat => combatSuffixes,
            SuffixType.Noble => nobleSuffixes,
            SuffixType.Mystic => mysticSuffixes,
            SuffixType.Furfolk => furfolkSuffixes,
            SuffixType.Featherfolk => featherfolkSuffixes,
            SuffixType.Scalefolk => scalefolkSuffixes,
            _ => genericSuffixes
        };
    }

    private static string GetRandomFromPool(string[] pool)
    {
        return pool[Random.Range(0, pool.Length)];
    }

    public static string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s)) return s;
        if (s.Length == 1) return s.ToUpper();
        return char.ToUpper(s[0]) + s.Substring(1);
    }
}