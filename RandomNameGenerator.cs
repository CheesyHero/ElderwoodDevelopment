
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
    // Compound Prefix Pools
    // --------------------

    private static readonly string[] genericPrefixes =
    {
        "fuzz", "sleepy", "wood", "run", "tree", "earth", "mud"
    };

    private static readonly string[] burrowguardPrefixes =
    {
        "stone", "root", "iron", "gold", "mud", "tunnel",
        "hammer", "steel", "dig", "dirt", "gem", "diamond"
    };

    private static readonly string[] treefolkPrefixes =
    {
        "sneak", "tip", "bark", "tree", "wind",
        "thief", "band", "shadow", "penny", "coin"
    };

    private static readonly string[] academyPrefixes =
    {
        "book", "wand", "spell", "hex", "whisper",
        "page", "carta", "pen", "ink", "high", "elder"
    };

    // --------------------
    // Compound Suffix Pools
    // --------------------

    private static readonly string[] genericSuffixes =
    {
        "tail", "foot", "paw", "eye", "ear",
        "kin", "step", "hand", "mark", "song",
        "shade", "crest", "heart", "path", "watch",
        "spark", "flare", "shade", "glow", "trace"
    };

    private static readonly string[] natureSuffixes =
    {
        "leaf", "tree", "ling", "root", "brush"
    };

    private static readonly string[] combatSuffixes =
    {
        "sword", "shield", "boot", "spear", "hammer",
        "claw", "tooth"
    };

    private static readonly string[] nobleSuffixes =
    {
        "king", "heart", "queen", "sword", "rad", "ward"
    };

    private static readonly string[] mysticSuffixes =
    {
        "star", "eye", "glyph", "moon", "sun"
    };

    private static readonly string[] furfolkSuffixes =
    {
        "paw", "fang", "claw", "tail",
        "fur", "mane", "tooth", "bite"
    };

    private static readonly string[] featherfolkSuffixes =
    {
        "wing", "claw", "talon", "crest", "beak",
        "quill", "down", "wind", "feather"
    };

    private static readonly string[] scalefolkSuffixes =
    {
        "scale", "fang", "hide", "crest",
        "spike", "fang", "bite", "hiss", "tail", "tooth"
    };

    // --------------------
    // Single Name Prefix Pools
    // --------------------

    private static readonly string[] genericNames =
    {
        "Lucky", "Pip", "Biscuit", "Pudding", "Nibbles",
        "Merry", "Sunny", "Button", "Penny", "Tumble",
        "Patches", "Muffin", "Pickles", "Wiggles", "Bumble"
    };

    private static readonly string[] burrowguardNames =
    {
        "Pebble", "Dusty", "Digger", "Rusty", "Nugget",
        "Gravel", "Boulder", "Copper", "Flint", "Chisel",
        "Truffle", "Mossy", "Bramble", "Tucker", "Clod",
        "Monte", "Diggydig", "Stinky", "Greasy", "Picker"
    };

    private static readonly string[] treefolkNames =
    {
        "Thistle", "Clover", "Fern", "Briar", "Sprig",
        "Twiggy", "Acorn", "Willow", "Hazel", "Tansy",
        "Bracken", "Juniper", "Petal", "Nettle", "Mistle", 
        "Sniff", "Dork", "Bub", "Chacha", "Knucklehead",
    };

    private static readonly string[] academyNames =
    {
        "Quill", "Rune", "Sage", "Wisp", "Glyph",
        "Ember", "Fable", "Riddle", "Scroll", "Inkwell",
        "Astrid", "Nimbus", "Oracle", "Vesper", "Sigil",
        "Zephyr", "Sonnet", "Cirrus", "Alto", "Stratus"
    };

    // --------------------
    // Single Name Suffix Pools
    // --------------------

    private static readonly string[] genericSuffixNames =
    {
        "Buddy", "Poppy", "Pipkin", "Waffles", "Doodle",
        "Toffee", "Tinker", "Bubbles", "Jolly", "Mallow",
        "Cricket", "Puddle", "Noodle", "Dandy", "Sparky",
    };

    private static readonly string[] natureNames =
    {
        "Daisy", "Blossom", "Maple", "Ivy", "Rosemary",
        "Basil", "Dewdrop", "Petunia", "Mushroom", "Sorrel",
        "Cypress", "Lavender", "Dandelion", "Heather", "Saffron"
    };

    private static readonly string[] combatNames =
    {
        "Spike", "Brutus", "Crusher", "Buster", "Rumble",
        "Bruiser", "Ripper", "Razor", "Clobber", "Smasher",
        "Axel", "Brawler", "Spar", "Grit", "Knuckles"
    };

    private static readonly string[] nobleNames =
    {
        "Reginald", "Percival", "Archibald", "Duchess", "Baron",
        "Winston", "Edmund", "Victoria", "Clarence", "Beatrice",
        "Sterling", "Ferdinand", "Cordelia", "Rupert", "Theodore"
    };

    private static readonly string[] mysticNames =
    {
        "Luna", "Nova", "Astral", "Myst", "Celeste",
        "Solstice", "Eclipse", "Aurora", "Stella", "Twilight",
        "Opal", "Specter", "Aether", "Cosmo", "Selene"
    };

    private static readonly string[] furfolkNames =
    {
        "Fluffy", "Fuzzy", "Whiskers", "Scruffy", "Tufty",
        "Furball", "Snuffles", "Paws", "Mittens", "Shaggy",
        "Velvet", "Fleece", "Woolly", "Sooty", "Fuzz",
        "Munchy", "Scruffy", "Stinky", "Smelly"
    };

    private static readonly string[] featherfolkNames =
    {
        "Feathers", "Chirpy", "Flitter", "Plucky", "Puffin",
        "Peepers", "Chipper", "Talon", "Flutter", "Plume",
        "Pip", "Squawk", "Flicker", "Skye", "Whistle",
        "Song", "Chirp", "Pecker", "Lovebird", "Dove",
    };

    private static readonly string[] scalefolkNames =
    {
        "Scales", "Slink", "Slither", "Hissy", "Flick",
        "Spiny", "Ember", "Sizzle", "Sparky", "Cinder",
        "Rascal", "Scorch", "Basilisk", "Skitter", "Snap",
        "Thorn", "Snitch", "Liz", "Lizzo", "Eliza", "Serpent"
    };

    // --------------------
    // Public Generator
    // --------------------

    public static string GenerateName(
        PrefixType prefixType,
        SuffixType suffixType)
    {
        // First coin flip: Single or compound name.
        bool useCompoundName = Random.Range(0, 3) == 1;

        if (useCompoundName)
        {
            string prefix = GetRandomFromPool(GetPrefixPool(prefixType));
            string suffix = GetRandomFromPool(GetSuffixPool(suffixType));

            return Capitalize(prefix) + suffix;
        }

        // Second coin flip: Prefix or suffix single-name pool.
        bool usePrefixNames = Random.Range(0, 2) == 0;

        string[] pool = usePrefixNames
            ? GetPrefixNamePool(prefixType)
            : GetSuffixNamePool(suffixType);

        return Capitalize(GetRandomFromPool(pool));
    }

    // --------------------
    // Compound Pool Selection
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

    // --------------------
    // Single Name Pool Selection
    // --------------------

    private static string[] GetPrefixNamePool(PrefixType type)
    {
        return type switch
        {
            PrefixType.Burrowguard => burrowguardNames,
            PrefixType.Treefolk => treefolkNames,
            PrefixType.Academy => academyNames,
            _ => genericNames
        };
    }

    private static string[] GetSuffixNamePool(SuffixType type)
    {
        return type switch
        {
            SuffixType.Nature => natureNames,
            SuffixType.Combat => combatNames,
            SuffixType.Noble => nobleNames,
            SuffixType.Mystic => mysticNames,
            SuffixType.Furfolk => furfolkNames,
            SuffixType.Featherfolk => featherfolkNames,
            SuffixType.Scalefolk => scalefolkNames,
            _ => genericSuffixNames
        };
    }

    // --------------------
    // Helpers
    // --------------------

    private static string GetRandomFromPool(string[] pool)
    {
        return pool[Random.Range(0, pool.Length)];
    }

    public static string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s))
            return s;

        if (s.Length == 1)
            return s.ToUpper();

        return char.ToUpper(s[0]) + s.Substring(1);
    }
}
