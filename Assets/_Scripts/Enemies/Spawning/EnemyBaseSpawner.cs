using System.Collections;
using UnityEngine;

public class EnemyBaseSpawner : MonoBehaviour
{
    [Header("Enemy Base")]
    [SerializeField] private EnemyShipData enemyBaseShipData;

    [Header("Spawn Locations")]
    [SerializeField] private Transform spawnPoint3;
    [SerializeField] private Transform spawnPoint9;

    [Header("Base Destination Locations")]
    [SerializeField] private Transform destinationPoint1;
    [SerializeField] private Transform destinationPoint2;

    [Header("Deployed Ship Destination")]
    [SerializeField] private Transform deployedShipDestination;

    [Header("Spawn Settings")]
    [SerializeField, Min(0f)] private float respawnDelay = 15f;
    [SerializeField] private bool spawnFromBothLocations;

    private EnemyBaseShip activeBase1;
    private EnemyBaseShip activeBase2;

    private Coroutine singleBaseRespawnCoroutine;

    private void Start()
    {
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
            SpawnBaseAtLane1();
        }
        else
        {
            SpawnBaseAtLane2();
        }
    }

    private void SpawnBaseAtLane1()
    {
        activeBase1 = SpawnBase(enemyBaseShipData, spawnPoint3, destinationPoint1, 1);
    }

    private void SpawnBaseAtLane2()
    {
        activeBase2 = SpawnBase(enemyBaseShipData, spawnPoint9, destinationPoint2, 2);
    }

    private EnemyBaseShip SpawnBase(EnemyShipData shipData, Transform spawnPoint, Transform destinationPoint, int lane)
    {
        if (shipData == null || shipData.ShipPrefab == null || spawnPoint == null || destinationPoint == null)
        {
            Debug.LogError($"Enemy Base Spawner is missing required Lane {lane} data.", this);
            return null;
        }

        GameObject spawnedBase = Instantiate(shipData.ShipPrefab, spawnPoint.position, spawnPoint.rotation);

        EnemyController enemyController = spawnedBase.GetComponent<EnemyController>();
        EnemyBaseShip enemyBaseShip = spawnedBase.GetComponent<EnemyBaseShip>();

        if (enemyController == null || enemyBaseShip == null)
        {
            Debug.LogError("Enemy Base prefab is missing EnemyController or EnemyBaseShip.", spawnedBase);
            Destroy(spawnedBase);
            return null;
        }

        enemyBaseShip.Initialize(this, lane, deployedShipDestination);
        enemyController.Initialize(shipData, spawnPoint, destinationPoint);

        return enemyBaseShip;
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