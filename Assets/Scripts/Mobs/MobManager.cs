using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton that tracks and manages all active mobs in the scene.
/// Provides a central point for querying, spawning, and controlling mobs globally.
/// </summary>
public class MobManager : MonoBehaviour
{
    // Singleton
    public static MobManager Instance { get; private set; }

    [Header("Spawn Settings")]
    [Tooltip("Mob prefabs available to spawn.")]
    public List<GameObject> mobPrefabs = new List<GameObject>();

    [Tooltip("Maximum number of mobs allowed in the scene at once.")]
    public int maxMobs = 50;

    [Tooltip("Terrain reference for placing mobs at correct height.")]
    public TerrainGenerator terrain;

    [Tooltip("Minimum distance from the player that mobs can spawn.")]
    public float minSpawnDistanceFromPlayer = 20f;

    [Tooltip("Maximum distance from the player that mobs can spawn.")]
    public float maxSpawnDistanceFromPlayer = 50f;

    // Active mob registry
    private List<MobBase> activeMobs = new List<MobBase>();

    // Player reference
    private Transform player;

    void Awake()
    {
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("MobManager: No GameObject tagged 'Player' found.");
    }

    // ─── Registry ────────────────────────────────────────────────────────────

    public void RegisterMob(MobBase mob)
    {
        if (!activeMobs.Contains(mob))
            activeMobs.Add(mob);
    }

    public void UnregisterMob(MobBase mob)
    {
        activeMobs.Remove(mob);
    }

    /// <summary>Returns a read-only view of all active mobs.</summary>
    public IReadOnlyList<MobBase> GetActiveMobs() => activeMobs.AsReadOnly();

    /// <summary>Returns the current number of active mobs.</summary>
    public int MobCount => activeMobs.Count;

    /// <summary>Returns true if more mobs can be spawned.</summary>
    public bool CanSpawnMore => activeMobs.Count < maxMobs;

    // ─── Spawning ────────────────────────────────────────────────────────────

    /// <summary>
    /// Spawns a mob prefab at a specific world position.
    /// </summary>
    public MobBase SpawnMob(GameObject prefab, Vector3 position)
    {
        if (!CanSpawnMore)
        {
            Debug.LogWarning("MobManager: Max mob count reached.");
            return null;
        }

        if (prefab == null)
        {
            Debug.LogWarning("MobManager: Prefab is null.");
            return null;
        }

        // Snap to terrain height
        if (terrain != null)
            position.y = terrain.GetHeightAt(position.x, position.z);

        GameObject instance = Instantiate(prefab, position, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
        MobBase mob = instance.GetComponent<MobBase>();

        if (mob == null)
            Debug.LogWarning($"MobManager: Prefab {prefab.name} has no MobBase component.");

        return mob;
    }

    /// <summary>
    /// Spawns a mob at a random position around the player within spawn distance range.
    /// </summary>
    public MobBase SpawnMobNearPlayer(GameObject prefab)
    {
        if (player == null) return null;

        Vector3 spawnPos = GetRandomSpawnPosition();
        return SpawnMob(prefab, spawnPos);
    }

    /// <summary>
    /// Despawns all active mobs immediately.
    /// </summary>
    public void DespawnAll()
    {
        for (int i = activeMobs.Count - 1; i >= 0; i--)
        {
            if (activeMobs[i] != null)
                Destroy(activeMobs[i].gameObject);
        }
        activeMobs.Clear();
    }

    // ─── Queries ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the closest mob to a world position, or null if none exist.
    /// </summary>
    public MobBase GetClosestMob(Vector3 position)
    {
        MobBase closest = null;
        float closestDist = float.MaxValue;

        foreach (var mob in activeMobs)
        {
            if (mob == null) continue;
            float dist = Vector3.Distance(position, mob.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = mob;
            }
        }

        return closest;
    }

    /// <summary>
    /// Returns all mobs within a given radius of a position.
    /// </summary>
    public List<MobBase> GetMobsInRange(Vector3 position, float radius)
    {
        List<MobBase> result = new List<MobBase>();
        foreach (var mob in activeMobs)
        {
            if (mob == null) continue;
            if (Vector3.Distance(position, mob.transform.position) <= radius)
                result.Add(mob);
        }
        return result;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    Vector3 GetRandomSpawnPosition()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minSpawnDistanceFromPlayer, maxSpawnDistanceFromPlayer);
            Vector3 candidate = player.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            if (terrain != null)
                candidate.y = terrain.GetHeightAt(candidate.x, candidate.z);

            // Make sure it's not too close to player
            if (Vector3.Distance(candidate, player.position) >= minSpawnDistanceFromPlayer)
                return candidate;
        }

        // Fallback
        return player.position + Vector3.forward * minSpawnDistanceFromPlayer;
    }

    void OnDrawGizmosSelected()
    {
        if (player == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, minSpawnDistanceFromPlayer);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, maxSpawnDistanceFromPlayer);
    }
}