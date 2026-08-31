using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Ships")]
    [SerializeField] private EnemyShipData[] enemyShipTypes;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Destination")]
    [SerializeField] private Transform destinationPoint;

    [Header("Spawn Settings")]
    [SerializeField, Min(0f)] private float initialSpawnDelay = 1f;
    [SerializeField, Min(0.1f)] private float timeBetweenSpawns = 3f;
    [SerializeField] private bool spawnContinuously = true;

    private Coroutine spawnCoroutine;

    private void Start()
    {
        if (!ValidateSpawner())
        {
            return;
        }

        spawnCoroutine = StartCoroutine(SpawnCycle());
    }

    private IEnumerator SpawnCycle()
    {
        if (initialSpawnDelay > 0f)
        {
            yield return new WaitForSeconds(initialSpawnDelay);
        }

        while (spawnContinuously)
        {
            SpawnRandomEnemy();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    private void SpawnRandomEnemy()
    {
        EnemyShipData selectedShipData = GetRandomShipData();
        Transform selectedSpawnPoint = GetRandomSpawnPoint();

        if (selectedShipData == null || selectedSpawnPoint == null)
        {
            return;
        }

        SpawnEnemy(selectedShipData, selectedSpawnPoint);
    }

    private void SpawnEnemy(EnemyShipData shipData, Transform spawnPoint)
    {
        if (shipData == null || shipData.ShipPrefab == null)
        {
            Debug.LogError("EnemySpawner received invalid EnemyShipData.", this);
            return;
        }

        if (spawnPoint == null || destinationPoint == null)
        {
            Debug.LogError("EnemySpawner is missing a spawn point or destination point.", this);
            return;
        }

        GameObject spawnedEnemy = Instantiate(shipData.ShipPrefab, spawnPoint.position, spawnPoint.rotation);

        EnemyController enemyController = spawnedEnemy.GetComponent<EnemyController>();

        if (enemyController == null)
        {
            Debug.LogError($"{spawnedEnemy.name} does not have an EnemyController.", spawnedEnemy);
            Destroy(spawnedEnemy);
            return;
        }

        enemyController.Initialize(shipData, spawnPoint, destinationPoint);
    }

    private EnemyShipData GetRandomShipData()
    {
        if (enemyShipTypes == null || enemyShipTypes.Length == 0)
        {
            return null;
        }

        return enemyShipTypes[Random.Range(0, enemyShipTypes.Length)];
    }

    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            return null;
        }

        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }

    private bool ValidateSpawner()
    {
        if (enemyShipTypes == null || enemyShipTypes.Length == 0)
        {
            Debug.LogError("EnemySpawner has no Enemy Ship Types assigned.", this);
            return false;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("EnemySpawner has no spawn points assigned.", this);
            return false;
        }

        if (destinationPoint == null)
        {
            Debug.LogError("EnemySpawner has no destination point assigned.", this);
            return false;
        }

        return true;
    }

    private void OnDestroy()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
}