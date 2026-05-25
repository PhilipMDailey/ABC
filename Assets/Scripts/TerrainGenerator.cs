using UnityEngine;
using Unity.AI.Navigation;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class TerrainGenerator : MonoBehaviour
{
    [Header("Mesh Settings")]
    [Tooltip("Number of vertices per side. Higher = more detail, more cost.")]
    public int resolution = 128;

    [Tooltip("World-space size of the terrain (square).")]
    public float size = 100f;

    [Header("Noise Settings")]
    [Tooltip("Scales the noise input — lower = broader features, higher = tighter detail.")]
    public float noiseScale = 0.05f;

    [Tooltip("Maximum height of terrain peaks.")]
    public float amplitude = 10f;

    [Tooltip("Shift the noise sample origin on X — acts as a horizontal seed.")]
    public float offsetX = 0f;

    [Tooltip("Shift the noise sample origin on Z — acts as a depth seed.")]
    public float offsetZ = 0f;

    [Header("Multi-Octave (optional)")]
    [Tooltip("How many noise layers to stack. More = richer detail.")]
    [Range(1, 6)]
    public int octaves = 3;

    [Tooltip("How much each successive octave contributes.")]
    [Range(0f, 1f)]
    public float persistence = 0.5f;

    [Tooltip("How much frequency increases per octave.")]
    public float lacunarity = 2f;

    [Header("NavMesh")]
    [Tooltip("NavMesh Surface component used to bake the navmesh at runtime.")]
    public NavMeshSurface navMeshSurface;

    private Mesh mesh;

    void Awake()
    {
        GenerateTerrain();
    }

    /// <summary>
    /// Call this to (re)generate the terrain mesh at any time.
    /// </summary>
    public void GenerateTerrain()
    {
        mesh = new Mesh();
        mesh.name = "ProceduralTerrain";
        mesh.MarkDynamic();

        if (resolution * resolution > 65535)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        GetComponent<MeshFilter>().mesh = mesh;

        BuildMesh(); // Build first

        // Now assign to collider after vertices exist
        MeshCollider meshCollider = GetComponent<MeshCollider>();

        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh baked successfully.");
        }
        else
        {
            Debug.LogWarning("TerrainGenerator: No NavMeshSurface assigned.");
        }

        if (meshCollider != null)
        {
            meshCollider.sharedMesh = null;
            meshCollider.sharedMesh = mesh;
            Debug.Log($"Mesh Collider assigned: {meshCollider.sharedMesh.vertexCount} vertices");
        }
        else
        {
            Debug.Log("No Mesh Collider found on Terrain GameObject");
        }
    }

    void BuildMesh()
    {
        int vertCount = resolution * resolution;
        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uvs = new Vector2[vertCount];
        int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

        float step = size / (resolution - 1);

        // Build vertices
        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                int i = z * resolution + x;
                float worldX = x * step;
                float worldZ = z * step;
                float y = SampleHeight(worldX, worldZ);

                vertices[i] = new Vector3(worldX, y, worldZ);
                uvs[i] = new Vector2((float)x / (resolution - 1), (float)z / (resolution - 1));
            }
        }

        // Build triangles
        int t = 0;
        for (int z = 0; z < resolution - 1; z++)
        {
            for (int x = 0; x < resolution - 1; x++)
            {
                int bl = z * resolution + x;
                int br = bl + 1;
                int tl = bl + resolution;
                int tr = tl + 1;

                // Triangle 1
                triangles[t++] = bl;
                triangles[t++] = tl;
                triangles[t++] = tr;

                // Triangle 2
                triangles[t++] = bl;
                triangles[t++] = tr;
                triangles[t++] = br;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    float SampleHeight(float worldX, float worldZ)
    {
        float height = 0f;
        float frequency = noiseScale;
        float amplitude = this.amplitude;
        float maxValue = 0f;

        for (int o = 0; o < octaves; o++)
        {
            float sampleX = (worldX + offsetX) * frequency;
            float sampleZ = (worldZ + offsetZ) * frequency;

            // Perlin noise returns [0,1]; remap to [-1,1] for symmetric hills/valleys
            float noise = Mathf.PerlinNoise(sampleX, sampleZ) * 2f - 1f;
            height += noise * amplitude;
            maxValue += amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        // Normalize so amplitude setting stays the true peak
        return height / maxValue * this.amplitude;
    }

    // Handy utility: given a world-space XZ position, returns the terrain height.
    // Useful later when you want to place objects on the surface.
    public float GetHeightAt(float worldX, float worldZ)
    {
        return SampleHeight(worldX - transform.position.x, worldZ - transform.position.z);
    }

#if UNITY_EDITOR
    // Lets you regenerate from the Inspector without entering Play mode.
    [ContextMenu("Regenerate Terrain")]
    void RegenerateInEditor()
    {
        GenerateTerrain();
    }
#endif
}