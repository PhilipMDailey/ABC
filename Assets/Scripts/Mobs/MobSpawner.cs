using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// Manages spawning of mobs per type, with independent control over
/// initial count, maximum active count, and spawn rate.
/// </summary>
public class MobSpawner : MonoBehaviour
{
    [System.Serializable]
    public class MobSpawnEntry
    {
        [Tooltip("The mob prefab to spawn.")]
        public GameObject prefab;

        [Tooltip("How many of this mob to spawn immediately on start.")]
        public int initialCount = 3;

        [Tooltip("Maximum number of this mob type allowed active at once. Spawning pauses when this is reached.")]
        public int maxCount = 10;

        [Tooltip("How many seconds between each spawn attempt once initial count is placed.")]
        public float spawnInterval = 5f;

        // Internal tracking
        [HideInInspector] public int activeCount = 0;
        [HideInInspector] public float spawnTimer = 0f;
    }

    [Header("Spawn Entries")]
    [Tooltip("Add one entry per mob type you want to manage.")]
    public List<MobSpawnEntry> spawnEntries = new List<MobSpawnEntry>();

    [Header("References")]
    [Tooltip("Terrain reference for height snapping.")]
    public TerrainGenerator terrain;

    [Tooltip("The player transform — mobs spawn at a distance from the player.")]
    public Transform player;

    [Header("Spawn Area")]
    [Tooltip("Minimum distance from the player that mobs can spawn.")]
    public float minSpawnDistance = 20f;

    [Tooltip("Maximum distance from the player that mobs can spawn.")]
    public float maxSpawnDistance = 50f;

    IEnumerator Start()
    {
        // Wait until NavMesh is ready
        while (UnityEngine.AI.NavMesh.CalculatePath(transform.position, transform.position, UnityEngine.AI.NavMesh.AllAreas, new UnityEngine.AI.NavMeshPath()) == false)
        {
            yield return null;
        }

        // Additional safety frame
        yield return new WaitForEndOfFrame();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
            else
                Debug.LogWarning("MobSpawner: No GameObject tagged 'Player' found.");
        }

        foreach (var entry in spawnEntries)
        {
            if (entry.prefab == null) continue;

            int toSpawn = Mathf.Min(entry.initialCount, entry.maxCount);
            for (int i = 0; i < toSpawn; i++)
                SpawnMob(entry);
        }
    }

    void Update()
    {
        foreach (var entry in spawnEntries)
        {
            if (entry.prefab == null) continue;

            // Skip if at max capacity
            if (entry.activeCount >= entry.maxCount) continue;

            // Tick the spawn timer
            entry.spawnTimer += Time.deltaTime;

            if (entry.spawnTimer >= entry.spawnInterval)
            {
                entry.spawnTimer = 0f;
                SpawnMob(entry);
            }
        }
    }

    void SpawnMob(MobSpawnEntry entry)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();

        // Snap to nearest valid NavMesh position
        UnityEngine.AI.NavMeshHit hit;
        if (!UnityEngine.AI.NavMesh.SamplePosition(spawnPosition, out hit, 10f, UnityEngine.AI.NavMesh.AllAreas))
        {
            Debug.LogWarning($"MobSpawner: Could not find valid NavMesh position near {spawnPosition}.");
            return;
        }

        spawnPosition = hit.position;

        GameObject instance = Instantiate(entry.prefab, spawnPosition, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));

        MobBase mob = instance.GetComponent<MobBase>();
        if (mob != null)
            mob.OnMobDestroyed += () => entry.activeCount--;
        else
            Debug.LogWarning($"MobSpawner: {entry.prefab.name} has no MobBase component.");

        entry.activeCount++;
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 origin = player != null ? player.position : transform.position;

        for (int i = 0; i < 30; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minSpawnDistance, maxSpawnDistance);
            Vector3 candidate = origin + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // Clamp to terrain bounds if terrain is assigned
            if (terrain != null)
            {
                candidate.x = Mathf.Clamp(candidate.x, terrain.transform.position.x, terrain.transform.position.x + terrain.size);
                candidate.z = Mathf.Clamp(candidate.z, terrain.transform.position.z, terrain.transform.position.z + terrain.size);
                candidate.y = terrain.GetHeightAt(candidate.x, candidate.z);
            }

            if (Vector3.Distance(candidate, origin) >= minSpawnDistance)
                return candidate;
        }

        return origin + Vector3.forward * minSpawnDistance;
    }

    void OnDrawGizmosSelected()
    {
        Vector3 origin = player != null ? player.position : transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(origin, minSpawnDistance);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, maxSpawnDistance);
    }
}