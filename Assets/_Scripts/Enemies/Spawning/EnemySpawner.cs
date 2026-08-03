using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Path")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform destinationPoint;

    [Header("Spawn")]
    [SerializeField, Min(0.1f)]
    private float spawnInterval = 3f;

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
        if (enemyPrefab == null ||
            spawnPoint == null ||
            destinationPoint == null)
        {
            Debug.LogWarning(
                "EnemySpawner is missing the Enemy Prefab, " +
                "Spawn Point, or Destination Point.",
                this
            );

            return;
        }

        GameObject enemy = Instantiate(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        EnemyPatrol patrol =
            enemy.GetComponent<EnemyPatrol>();

        if (patrol == null)
        {
            Debug.LogWarning(
                $"{enemy.name} does not contain EnemyPatrol.",
                enemy
            );

            return;
        }

        patrol.Initialize(
            spawnPoint,
            destinationPoint
        );
    }
}