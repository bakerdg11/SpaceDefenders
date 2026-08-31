using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private EnemyShipData shipData;
    private Transform startPoint;
    private Transform destinationPoint;

    private Vector3 launchTargetPosition;

    private bool hasBeenInitialized;
    private bool hasReachedDestination;
    private bool isLaunching;

    public EnemyShipData ShipData => shipData;
    public bool HasReachedDestination => hasReachedDestination;

    private void Update()
    {
        if (!hasBeenInitialized || hasReachedDestination)
        {
            return;
        }

        if (isLaunching)
        {
            MoveToLaunchTarget();
            return;
        }

        MoveToDestination();
    }

    public void Initialize(EnemyShipData newShipData, Transform newStartPoint, Transform newDestinationPoint)
    {
        if (newShipData == null || newStartPoint == null || newDestinationPoint == null)
        {
            Debug.LogError($"{name} received invalid enemy initialization data.", this);
            return;
        }

        shipData = newShipData;

        EnemyHealth enemyHealth = GetComponent<EnemyHealth>();

        if (enemyHealth != null)
        {
            enemyHealth.Initialize(shipData);
        }

        startPoint = newStartPoint;
        destinationPoint = newDestinationPoint;

        transform.position = startPoint.position;

        hasReachedDestination = false;
        hasBeenInitialized = true;
        isLaunching = false;
    }

    public void InitializeDeployment(EnemyShipData newShipData, Transform newStartPoint, Transform newDestinationPoint, Vector3 newLaunchTargetPosition)
    {
        Initialize(newShipData, newStartPoint, newDestinationPoint);

        launchTargetPosition = newLaunchTargetPosition;
        isLaunching = true;
    }

    private void MoveToLaunchTarget()
    {
        Vector3 direction = launchTargetPosition - transform.position;
        direction.y = 0f;

        RotateToward(direction);

        transform.position = Vector3.MoveTowards(transform.position, launchTargetPosition, shipData.MovementSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, launchTargetPosition) <= shipData.StoppingDistance)
        {
            transform.position = launchTargetPosition;
            isLaunching = false;
        }
    }

    private void MoveToDestination()
    {
        Vector3 direction = destinationPoint.position - transform.position;
        direction.y = 0f;

        RotateToward(direction);

        transform.position = Vector3.MoveTowards(transform.position, destinationPoint.position, shipData.MovementSpeed * Time.deltaTime);

        float distanceToDestination = Vector3.Distance(transform.position, destinationPoint.position);

        if (distanceToDestination <= shipData.StoppingDistance)
        {
            transform.position = destinationPoint.position;
            hasReachedDestination = true;

            EnemyBaseShip enemyBaseShip = GetComponent<EnemyBaseShip>();

            if (enemyBaseShip != null)
            {
                enemyBaseShip.BeginDeployment();
            }
        }
    }

    private void RotateToward(Vector3 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, shipData.RotationSpeed * Time.deltaTime);
    }
}