using UnityEngine;

public class RunMapManager : MonoBehaviour
{
    public static RunMapData CurrentMap { get; private set; }

    [Header("Generation")]
    public RunMapGenerator.Settings settings = new RunMapGenerator.Settings();
    public bool useFixedSeed = false;
    public int fixedSeed = 12345;

    [Header("Debug")]
    public bool generateOnAwake = true;

    private const string SaveKey = "RUN_MAP_JSON";

    void Awake()
    {
        if (generateOnAwake)
            NewRun();
    }

    [ContextMenu("New Run (Generate Map)")]
    public void NewRun()
    {
        int? seed = useFixedSeed ? fixedSeed : (int?)null;

        CurrentMap = RunMapGenerator.Generate(settings, seed);

        // Store (so we can reload this run later)
        string json = JsonUtility.ToJson(CurrentMap);
        PlayerPrefs.SetString(SaveKey, json);
        PlayerPrefs.Save();

        Debug.Log($"Generated map. Seed={CurrentMap.seed} Nodes={CurrentMap.nodes.Count} Edges={CurrentMap.edges.Count}");
    }

    [ContextMenu("Load Run (From Saved JSON)")]
    public void LoadRun()
    {
        if (!PlayerPrefs.HasKey(SaveKey))
        {
            Debug.LogWarning("No saved run map found.");
            return;
        }

        string json = PlayerPrefs.GetString(SaveKey);
        CurrentMap = JsonUtility.FromJson<RunMapData>(json);

        Debug.Log($"Loaded map. Seed={CurrentMap.seed} Nodes={CurrentMap.nodes.Count} Edges={CurrentMap.edges.Count}");
    }
}
