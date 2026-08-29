using System.Collections;
using UnityEngine;

public class EnemyBaseSpawner : MonoBehaviour
{
    [Header("Enemy Base")]
    [SerializeField] private GameObject enemyBaseShipPrefab;

    [Header("Spawn Locations")]
    [SerializeField] private Transform spawnPoint3;
    [SerializeField] private Transform spawnPoint9;

    [Header("Destination Locations")]
    [SerializeField] private Transform destinationPoint1;
    [SerializeField] private Transform destinationPoint2;

    [Header("Spawn Settings")]
    [SerializeField, Min(0f)] private float respawnDelay = 15f;
    [SerializeField] private bool spawnFromBothLocations = false;

    private EnemyBaseShip activeBase1;
    private EnemyBaseShip activeBase2;
    private Coroutine singleBaseRespawnCoroutine;

    private void Start()
    {
        Debug.Log("EnemyBaseSpawner Start is running.", this);

        if (spawnFromBothLocations)
        {
            SpawnBaseAtLane1();
            SpawnBaseAtLane2();
        }
        else
        {
            SpawnRandomBase();
        }
    }

    private void SpawnRandomBase()
    {
        if (Random.Range(0, 2) == 0)
        {
            Debug.Log("Enemy Base selected Lane 1.", this);
            SpawnBaseAtLane1();
        }
        else
        {
            Debug.Log("Enemy Base selected Lane 2.", this);
            SpawnBaseAtLane2();
        }
    }

    private void SpawnBaseAtLane1()
    {
        if (enemyBaseShipPrefab == null || spawnPoint3 == null || destinationPoint1 == null)
        {
            Debug.LogError("Enemy Base Spawner is missing a Lane 1 reference.", this);
            return;
        }

        GameObject spawnedBase = Instantiate(enemyBaseShipPrefab, spawnPoint3.position, spawnPoint3.rotation);

        EnemyPatrol enemyPatrol = spawnedBase.GetComponent<EnemyPatrol>();
        EnemyBaseShip enemyBaseShip = spawnedBase.GetComponent<EnemyBaseShip>();

        if (enemyPatrol == null || enemyBaseShip == null)
        {
            Debug.LogError("Enemy Base Ship prefab is missing EnemyPatrol or EnemyBaseShip.", spawnedBase);
            Destroy(spawnedBase);
            return;
        }

        activeBase1 = enemyBaseShip;

        enemyBaseShip.Initialize(this, 1);
        enemyPatrol.Initialize(spawnPoint3, destinationPoint1);
    }

    private void SpawnBaseAtLane2()
    {
        if (enemyBaseShipPrefab == null || spawnPoint9 == null || destinationPoint2 == null)
        {
            Debug.LogError("Enemy Base Spawner is missing a Lane 2 reference.", this);
            return;
        }

        GameObject spawnedBase = Instantiate(enemyBaseShipPrefab, spawnPoint9.position, spawnPoint9.rotation);

        EnemyPatrol enemyPatrol = spawnedBase.GetComponent<EnemyPatrol>();
        EnemyBaseShip enemyBaseShip = spawnedBase.GetComponent<EnemyBaseShip>();

        if (enemyPatrol == null || enemyBaseShip == null)
        {
            Debug.LogError("Enemy Base Ship prefab is missing EnemyPatrol or EnemyBaseShip.", spawnedBase);
            Destroy(spawnedBase);
            return;
        }

        activeBase2 = enemyBaseShip;

        enemyBaseShip.Initialize(this, 2);
        enemyPatrol.Initialize(spawnPoint9, destinationPoint2);
    }

    public void NotifyBaseDestroyed(int lane)
    {
        if (lane == 1)
        {
            activeBase1 = null;
        }
        else if (lane == 2)
        {
            activeBase2 = null;
        }

        if (spawnFromBothLocations)
        {
            StartCoroutine(RespawnLaneAfterDelay(lane));
        }
        else if (singleBaseRespawnCoroutine == null)
        {
            singleBaseRespawnCoroutine = StartCoroutine(RespawnSingleBaseAfterDelay());
        }
    }

    private IEnumerator RespawnSingleBaseAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        SpawnRandomBase();
        singleBaseRespawnCoroutine = null;
    }

    private IEnumerator RespawnLaneAfterDelay(int lane)
    {
        yield return new WaitForSeconds(respawnDelay);

        if (lane == 1 && activeBase1 == null)
        {
            SpawnBaseAtLane1();
        }
        else if (lane == 2 && activeBase2 == null)
        {
            SpawnBaseAtLane2();
        }
    }
}