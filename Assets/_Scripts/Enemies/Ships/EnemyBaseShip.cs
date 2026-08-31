using System.Collections;
using UnityEngine;

public class EnemyBaseShip : MonoBehaviour
{
    [Header("Deployment")]
    [SerializeField] private EnemyShipData deployedShipData;
    [SerializeField] private Transform[] deploymentPoints;
    [SerializeField, Min(0f)] private float launchDistance = 5f;

    [Header("Deployment Timing")]
    [SerializeField, Min(0f)] private float timeBetweenDeployments = 1f;
    [SerializeField, Min(0f)] private float timeBetweenWaves = 5f;

    private EnemyBaseSpawner spawner;
    private Transform deployedShipDestination;
    private int lane;

    private bool destructionReported;
    private bool deploymentStarted;

    private Coroutine deploymentCoroutine;

    public void Initialize(EnemyBaseSpawner newSpawner, int newLane, Transform newDeployedShipDestination)
    {
        spawner = newSpawner;
        lane = newLane;
        deployedShipDestination = newDeployedShipDestination;
    }

    public void BeginDeployment()
    {
        if (deploymentStarted)
        {
            return;
        }

        if (deployedShipData == null || deployedShipData.ShipPrefab == null)
        {
            Debug.LogError($"{name} has no valid deployed Enemy Ship Data assigned.", this);
            return;
        }

        if (deploymentPoints == null || deploymentPoints.Length == 0)
        {
            Debug.LogError($"{name} has no deployment points assigned.", this);
            return;
        }

        if (deployedShipDestination == null)
        {
            Debug.LogError($"{name} has no deployed ship destination assigned.", this);
            return;
        }

        deploymentStarted = true;
        deploymentCoroutine = StartCoroutine(DeploymentCycle());
    }

    private IEnumerator DeploymentCycle()
    {
        while (true)
        {
            for (int i = 0; i < deploymentPoints.Length; i++)
            {
                Transform deploymentPoint = deploymentPoints[i];

                if (deploymentPoint != null)
                {
                    SpawnDeployedShip(deploymentPoint, i);
                }

                if (i < deploymentPoints.Length - 1)
                {
                    yield return new WaitForSeconds(timeBetweenDeployments);
                }
            }

            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    private void SpawnDeployedShip(Transform deploymentPoint, int deploymentIndex)
    {
        GameObject spawnedShip = Instantiate(deployedShipData.ShipPrefab, deploymentPoint.position, deploymentPoint.rotation);

        EnemyController enemyController = spawnedShip.GetComponent<EnemyController>();

        if (enemyController == null)
        {
            Debug.LogError($"{spawnedShip.name} does not have an EnemyController.", spawnedShip);
            Destroy(spawnedShip);
            return;
        }

        Vector3 launchDirection;

        if (deploymentIndex <= 2)
        {
            launchDirection = transform.right;
        }
        else
        {
            launchDirection = -transform.right;
        }

        Vector3 launchTargetPosition = deploymentPoint.position + launchDirection * launchDistance;

        enemyController.InitializeDeployment(deployedShipData, deploymentPoint, deployedShipDestination, launchTargetPosition);
    }

    public void NotifyDestroyed()
    {
        if (destructionReported)
        {
            return;
        }

        destructionReported = true;

        if (deploymentCoroutine != null)
        {
            StopCoroutine(deploymentCoroutine);
            deploymentCoroutine = null;
        }

        if (spawner != null)
        {
            spawner.NotifyBaseDestroyed(lane);
        }
    }
}