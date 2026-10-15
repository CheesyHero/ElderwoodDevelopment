using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelectMap : MonoBehaviour
{
    public static World World { get { return GameManager.Player.currentWorld; } }
    public static RunMapData Map { get { return GameManager.CurrentMap; } }

    [Header("UI Container")]
    [SerializeField] private RectTransform mapRect;          // the rectangle that contains the map
    [SerializeField] private RectTransform nodesParent;      // optional child container (null -> uses mapRect)
    [SerializeField] private float padding = 30f;            // keep nodes away from edges

    [Header("Node Prefabs (Aesthetic Variants)")]
    public MapNode mainNode;   // used for normal nodes
    public MapNode startNode;  // used for start node
    public MapNode endNode;    // used for boss/end node 

    private readonly Dictionary<int, MapNode> _spawned = new();

    [Header("Lines")]
    [SerializeField] private RectTransform linesParent; // optional (null -> uses mapRect)
    [SerializeField] private Image linePrefab;          // a simple UI Image (1x1 sprite is fine)
    [SerializeField] private float lineThickness = 6f;
    [SerializeField] private float lineEndPadding = 18f; // keeps lines from overlapping node centers

    private readonly List<GameObject> _spawnedLines = new();

    private void OnEnable()
    {
        Render();
    }
    public void Render()
    { 
        Clear();
        Render(Map);
        DrawEdges(Map);
    }

    public void Render(RunMapData map)
    {
        if (map == null) return;
        if (mapRect == null) { Debug.LogWarning("LevelSelectMap: mapRect not assigned."); return; }
        if (mainNode == null || startNode == null || endNode == null)
        {
            Debug.LogWarning("LevelSelectMap: Assign mainNode/startNode/endNode prefabs.");
            return;
        }

        RectTransform parent = nodesParent != null ? nodesParent : mapRect;

        // Compute generator-space bounds
        float minX = float.PositiveInfinity, maxX = float.NegativeInfinity;
        float minY = float.PositiveInfinity, maxY = float.NegativeInfinity;

        for (int i = 0; i < map.nodes.Count; i++)
        {
            var n = map.nodes[i];
            if (n.position.x < minX) minX = n.position.x;
            if (n.position.x > maxX) maxX = n.position.x;
            if (n.position.y < minY) minY = n.position.y;
            if (n.position.y > maxY) maxY = n.position.y;
        }

        float usableW = Mathf.Max(1f, mapRect.rect.width - padding * 2f);
        float usableH = Mathf.Max(1f, mapRect.rect.height - padding * 2f);
        float spanX = Mathf.Max(1f, maxX - minX);
        float spanY = Mathf.Max(1f, maxY - minY);

        // Spawn nodes
        for (int i = 0; i < map.nodes.Count; i++)
        {
            MapNodeData data = map.nodes[i];

            MapNode prefab = GetPrefabForType(data.type);
            MapNode node = Instantiate(prefab, parent);
            node.name = $"MapNode_{data.id}_{data.type}";
            node.data = data;

            // Normalize to 0..1 then remap to rect centered at (0,0)
            float nx = (data.position.x - minX) / spanX;
            float ny = (data.position.y - minY) / spanY;

            float px = (-usableW * 0.5f) + (nx * usableW);
            float py = (-usableH * 0.5f) + (ny * usableH);

            RectTransform rt = node.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = new Vector2(px, py);
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one;
            }
            else
            {
                node.transform.localPosition = new Vector3(px, py, 0f);
                node.transform.localRotation = Quaternion.identity;
                node.transform.localScale = Vector3.one;
            }

            _spawned[data.id] = node; 
        }
    }

    private MapNode GetPrefabForType(MapNodeType type)
    {
        return type switch
        {
            MapNodeType.Start => startNode,
            MapNodeType.Boss => endNode,
            _ => mainNode,
        };
    }

    [ContextMenu("Clear")]
    public void Clear()
    {
        RectTransform parent = nodesParent != null ? nodesParent : mapRect;
        if (parent == null) return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);
            if (child.GetComponent<MapNode>() != null)
                DestroyImmediate(child.gameObject);
        }

        _spawned.Clear();
    }

    // Call this after Render() spawns nodes
    private void DrawEdges(RunMapData map)
    {
        if (map == null) return;
        if (linePrefab == null)
        {
            Debug.LogWarning("LevelSelectMap: linePrefab not assigned.");
            return;
        }

        RectTransform parent = linesParent != null ? linesParent : mapRect;

        // Clear old lines
        for (int i = 0; i < _spawnedLines.Count; i++)
            DestroyImmediate(_spawnedLines[i]);
        _spawnedLines.Clear();

        // Draw each edge
        foreach (var e in map.edges)
        {
            if (!_spawned.TryGetValue(e.fromId, out MapNode fromNode)) continue;
            if (!_spawned.TryGetValue(e.toId, out MapNode toNode)) continue;

            RectTransform aRt = fromNode.GetComponent<RectTransform>();
            RectTransform bRt = toNode.GetComponent<RectTransform>();
            if (aRt == null || bRt == null) continue;

            Vector2 a = aRt.anchoredPosition;
            Vector2 b = bRt.anchoredPosition;

            CreateLine(parent, a, b);
        }

        // Ensure lines appear behind nodes
        parent.SetAsFirstSibling();
    }

    private void CreateLine(RectTransform parent, Vector2 a, Vector2 b)
    {
        Vector2 dir = (b - a);
        float dist = dir.magnitude;
        if (dist <= 0.001f) return;

        Vector2 n = dir / dist;

        // Pull endpoints in a bit so the line doesn't sit on top of the node art
        a += n * lineEndPadding;
        b -= n * lineEndPadding;

        dir = (b - a);
        dist = dir.magnitude;
        if (dist <= 0.001f) return;

        Vector2 mid = (a + b) * 0.5f;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        Image img = Instantiate(linePrefab, parent);
        img.name = "MapLine";
        RectTransform rt = img.rectTransform;

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = mid;
        rt.sizeDelta = new Vector2(dist, lineThickness);
        rt.localRotation = Quaternion.Euler(0f, 0f, angle);
        rt.localScale = Vector3.one;

        _spawnedLines.Add(img.gameObject);
    }
}
