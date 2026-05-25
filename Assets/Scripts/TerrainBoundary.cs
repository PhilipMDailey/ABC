using UnityEngine;

/// <summary>
/// Creates invisible walls around the terrain edges to prevent
/// characters from falling off. Temporary solution for prototyping.
/// </summary>
public class TerrainBoundary : MonoBehaviour
{
    [Header("Terrain Reference")]
    public TerrainGenerator terrain;

    [Tooltip("How tall the invisible walls are.")]
    public float wallHeight = 50f;

    [Tooltip("How thick the invisible walls are.")]
    public float wallThickness = 2f;

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogWarning("TerrainBoundary: No TerrainGenerator assigned.");
            return;
        }

        BuildWalls();
    }

    void BuildWalls()
    {
        float size = terrain.size;
        float halfSize = size / 2f;
        float halfThickness = wallThickness / 2f;
        float halfHeight = wallHeight / 2f;
        Vector3 origin = terrain.transform.position;

        // North, South, East, West walls
        CreateWall("Wall_North", origin + new Vector3(halfSize, halfHeight, size + halfThickness), new Vector3(size + wallThickness * 2f, wallHeight, wallThickness));
        CreateWall("Wall_South", origin + new Vector3(halfSize, halfHeight, -halfThickness),       new Vector3(size + wallThickness * 2f, wallHeight, wallThickness));
        CreateWall("Wall_East",  origin + new Vector3(size + halfThickness, halfHeight, halfSize), new Vector3(wallThickness, wallHeight, size));
        CreateWall("Wall_West",  origin + new Vector3(-halfThickness, halfHeight, halfSize),       new Vector3(wallThickness, wallHeight, size));
    }

    void CreateWall(string wallName, Vector3 position, Vector3 size)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.SetParent(transform);
        wall.transform.position = position;

        BoxCollider col = wall.AddComponent<BoxCollider>();
        col.size = size;
    }

#if UNITY_EDITOR
    [ContextMenu("Rebuild Walls")]
    void RebuildWalls()
    {
        // Remove existing walls
        for (int i = transform.childCount - 1; i >= 0; i--)
            DestroyImmediate(transform.GetChild(i).gameObject);

        BuildWalls();
    }
#endif
}