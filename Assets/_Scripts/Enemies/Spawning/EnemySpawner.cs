using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Path")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Transform destinationPoint;

    [Header("Spawn")]
    [SerializeField] private float spawnInterval = 3f;

    private float spawnTimer;

    private void Start()
    {
        SpawnEnemy();
    }

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        if (enemyPrefab == null || spawnPoints == null || spawnPoints.Length == 0 || destinationPoint == null)
        {
            Debug.LogWarning("EnemySpawner is missing the Enemy Prefab, Spawn Points, or Destination Point.", this);
            return;
        }

        Transform selectedSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

        if (selectedSpawnPoint == null)
        {
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab, selectedSpawnPoint.position, selectedSpawnPoint.rotation);

        EnemyPatrol patrol = enemy.GetComponent<EnemyPatrol>();

        if (patrol == null)
        {
            Debug.LogWarning($"{enemy.name} does not contain EnemyPatrol.", enemy);
            return;
        }

        patrol.Initialize(selectedSpawnPoint, destinationPoint);
    }
}