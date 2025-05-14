using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ZombieHordeManager : NetworkBehaviour
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
    public float minSpawnDistance = 40f;
    public float maxSpawnDistance = 60f;
    public float playerFOV = 90f;

    private int currentHorde = 1;
    private Transform player;

    void Start()
    {
        InitPools();
        StartCoroutine(HordeLoop());

    }

    private void UpdatePlayerList()
    {
        players = new List<Transform>(CleanMoreNetworkManager.AllPlayers);

        if (players.Count > 0)
        {
            player = players[0]; // o usar l?gica para elegir uno
        }
    }


    void InitPools()
    {
        foreach (var entry in zombieTypes)
        {
            for (int i = 0; i < entry.initialSize; i++)
            {
                GameObject z = Instantiate(entry.prefab, transform);
                z.SetActive(false); // NO hacer Spawn aqu?
                entry.pool.Add(z);
            }
        }
    }


    IEnumerator HordeLoop()
    {
        yield return new WaitForSeconds(5f);
        UpdatePlayerList();
        print("new horde");
        SpawnHorde();
        currentHorde++;
        yield return new WaitForSeconds(timeBetweenHordes);
        StartCoroutine(HordeLoop());
    }

    void SpawnHorde()
    {
        if (!isServer) return;

        int totalZombies = baseZombiesPerHorde + currentHorde * 2;

        for (int i = 0; i < totalZombies; i++)
        {
            ZombieType type = GetRandomZombieType();
            GameObject zombie = GetZombieFromPool(type);

            if (zombie == null) continue;

            zombie.transform.position = GetSafeSpawnPosition();
            zombie.transform.rotation = Quaternion.identity;

            zombie.SetActive(true);              // Activar localmente en el servidor
            NetworkServer.Spawn(zombie);         // Hacerlo visible a todos los jugadores
        }

    }

    ZombieType GetRandomZombieType()
    {
        float roll = Random.value;
        if (currentHorde < 3 || roll < 0.6f) return ZombieType.Normal;
        if (currentHorde < 5 && roll < 0.75f) return ZombieType.Rapido;
        if (currentHorde < 7 && roll < 0.9f) return ZombieType.Rapido;
        return ZombieType.Tanque;
    }

    GameObject GetZombieFromPool(ZombieType type)
    {
        var entry = zombieTypes.Find(e => e.type == type);
        if (entry == null)
        {
            Debug.LogError("No se encontr? entrada de pool para zombie type: " + type);
            return null;
        }

        foreach (var z in entry.pool)
        {
            if (!z.activeInHierarchy) return z;  // Devuelve un zombie desactivado del pool
        }

        // Expandir el pool si no hay disponibles
        GameObject newZ = Instantiate(entry.prefab, transform);
        newZ.SetActive(false); // importante: el nuevo zombie inicia inactivo
        entry.pool.Add(newZ);
        return newZ;
    }


    Vector3 GetSafeSpawnPosition()
{
        if (players == null || players.Count == 0)
        {
            Debug.LogError("No hay jugadores en la lista de 'players'. No se puede generar posici?n de spawn segura.");
            return Vector3.zero; // O un valor por defecto seguro
        }

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

        Vector3 fallback = players[0].position - players[0].forward * minSpawnDistance;
        fallback.y = players[0].position.y;
        return fallback;
}


    bool IsBehindPlayer(Vector3 pos)
    {
        Vector3 toPos = (pos - player.position).normalized;
        float angle = Vector3.Angle(player.forward, toPos);
        return angle > playerFOV / 2f;
    }
}
