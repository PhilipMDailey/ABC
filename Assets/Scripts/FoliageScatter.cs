using UnityEngine;
using System.Collections.Generic;

public class FoliageScatter : MonoBehaviour
{
    [Header("Terrain Reference")]
    [Tooltip("The TerrainGenerator this scatterer will place foliage on.")]
    public TerrainGenerator terrain;

    [System.Serializable]
    public class FoliageEntry
    {
        [Tooltip("The prefab to scatter.")]
        public GameObject prefab;

        [Header("Scatter Settings")]
        [Tooltip("How many of this prefab to spawn.")]
        public int count = 50;

        [Tooltip("Minimum scale multiplier.")]
        public float minScale = 0.8f;

        [Tooltip("Maximum scale multiplier.")]
        public float maxScale = 1.2f;

        [Tooltip("Random seed for this prefab's scatter pattern.")]
        public int seed = 42;

        [Header("Height Filter")]
        [Tooltip("Only place this prefab above this terrain height.")]
        public float minHeight = float.MinValue;

        [Tooltip("Only place this prefab below this terrain height.")]
        public float maxHeight = float.MaxValue;
    }

    [Header("Foliage Prefabs")]
    [Tooltip("Add prefabs here. Each has its own independent settings.")]
    public List<FoliageEntry> foliageEntries = new List<FoliageEntry>();

    private GameObject scatterRoot;

    void Start()
    {
        Scatter();
    }

    public void Scatter()
    {
        // Clean up previous scatter
        if (scatterRoot != null)
            Destroy(scatterRoot);

        if (terrain == null)
        {
            Debug.LogWarning("FoliageScatter: No TerrainGenerator assigned.");
            return;
        }

        if (foliageEntries == null || foliageEntries.Count == 0)
        {
            Debug.LogWarning("FoliageScatter: No foliage prefabs assigned.");
            return;
        }

        scatterRoot = new GameObject("FoliageScatter_Root");
        scatterRoot.transform.SetParent(transform);

        float terrainSize = terrain.size;
        Vector3 terrainOrigin = terrain.transform.position;

        // Process each entry independently
        foreach (var entry in foliageEntries)
        {
            if (entry.prefab == null)
            {
                Debug.LogWarning("FoliageScatter: A prefab entry is null, skipping.");
                continue;
            }

            // Each entry gets its own seed so they have independent scatter patterns
            Random.InitState(entry.seed);

            // Create a parent object per prefab type to keep Hierarchy clean
            GameObject entryRoot = new GameObject($"Scatter_{entry.prefab.name}");
            entryRoot.transform.SetParent(scatterRoot.transform);

            int placed = 0;
            int attempts = 0;
            int maxAttempts = entry.count * 10;

            while (placed < entry.count && attempts < maxAttempts)
            {
                attempts++;

                float x = Random.Range(0f, terrainSize) + terrainOrigin.x;
                float z = Random.Range(0f, terrainSize) + terrainOrigin.z;
                float y = terrain.GetHeightAt(x, z) + terrainOrigin.y;

                // Height filter
                if (y < entry.minHeight || y > entry.maxHeight)
                    continue;

                float rotY = Random.Range(0f, 360f);
                float scale = Random.Range(entry.minScale, entry.maxScale);

                GameObject instance = Instantiate(
                    entry.prefab,
                    new Vector3(x, y, z),
                    Quaternion.Euler(0f, rotY, 0f)
                );

                instance.transform.localScale = entry.prefab.transform.localScale * scale;
                instance.transform.SetParent(entryRoot.transform);
                instance.name = entry.prefab.name;

                placed++;
            }

            if (placed < entry.count)
                Debug.LogWarning($"FoliageScatter: Only placed {placed}/{entry.count} for {entry.prefab.name} — height filter may be too restrictive.");
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Scatter Foliage")]
    void ScatterInEditor()
    {
        Scatter();
    }
#endif
}