using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieHordeManager : MonoBehaviour
{
    public enum ZombieType { Normal, Rapido, Distancia, Tanque }

    private List<Transform> players = new List<Transform>();

    [System.Serializable]
    public class ZombieEntry
    {
        public ZombieType type;
        public GameObject prefab;
        public int initialSize = 10;
        [HideInInspector] public List<GameObject> pool = new List<GameObject>();
    }

    public List<ZombieEntry> zombieTypes;
    public int baseZombiesPerHorde = 5;
    public float timeBetweenHordes = 10f;
    public float minSpawnDistance = 10f;
    public float maxSpawnDistance = 25f;
    public float playerFOV = 90f;

    private int currentHorde = 1;
    private Transform player;

    void Start()
    {
        GameObject[] playerObjs = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject obj in playerObjs)
        {
            players.Add(obj.transform);
        }

        InitPools();
        StartCoroutine(HordeLoop());

    }

    void InitPools()
    {
        foreach (var entry in zombieTypes)
        {
            for (int i = 0; i < entry.initialSize; i++)
            {
                GameObject z = Instantiate(entry.prefab);
                z.SetActive(false);
                entry.pool.Add(z);
            }
        }
    }

    IEnumerator HordeLoop()
    {
        yield return new WaitForSeconds(timeBetweenHordes);
        print("new horde");
        SpawnHorde();
        currentHorde++;
    }

    void SpawnHorde()
    {
        int totalZombies = baseZombiesPerHorde + currentHorde * 2;

        for (int i = 0; i < totalZombies; i++)
        {
            ZombieType type = GetRandomZombieType();
            GameObject zombie = GetZombieFromPool(type);
            zombie.transform.position = GetSafeSpawnPosition();
            zombie.SetActive(true);
        }
    }

    ZombieType GetRandomZombieType()
    {
        float roll = Random.value;
        if (roll < 0.6f) return ZombieType.Normal;
        if (currentHorde < 5 && roll < 0.75f) return ZombieType.Rapido;
        if (currentHorde < 7 && roll < 0.9f) return ZombieType.Distancia;
        return ZombieType.Tanque;
    }

    GameObject GetZombieFromPool(ZombieType type)
    {
        var entry = zombieTypes.Find(e => e.type == type);
        foreach (var z in entry.pool)
        {
            if (!z.activeInHierarchy) return z;
        }

        // Expand pool if needed
        GameObject newZ = Instantiate(entry.prefab);
        newZ.SetActive(false);
        entry.pool.Add(newZ);
        return newZ;
    }

    Vector3 GetSafeSpawnPosition()
{
    for (int i = 0; i < 30; i++)
    {
        Vector3 dir = Random.onUnitSphere;
        dir.y = 0;
        Vector3 candidate = players[Random.Range(0, players.Count)].position + dir.normalized * Random.Range(minSpawnDistance, maxSpawnDistance);

        bool isSafe = true;

        foreach (var p in players)
        {
            float dist = Vector3.Distance(candidate, p.position);
            if (dist < minSpawnDistance || !IsBehindPlayer(candidate))
            {
                isSafe = false;
                break;
            }
        }

        if (isSafe) return candidate;
    }

    // Fallback
    return players[0].position + Random.onUnitSphere * maxSpawnDistance;
}


    bool IsBehindPlayer(Vector3 pos)
    {
        Vector3 toPos = (pos - player.position).normalized;
        float angle = Vector3.Angle(player.forward, toPos);
        return angle > playerFOV / 2f;
    }
}
