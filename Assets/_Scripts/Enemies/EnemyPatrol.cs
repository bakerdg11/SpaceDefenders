using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField, Min(0.1f)]
    private float movementSpeed = 3f;

    [SerializeField, Min(0.01f)]
    private float stoppingDistance = 0.1f;

    [SerializeField, Min(0f)]
    private float rotationSpeed = 360f;

    private Transform startPoint;
    private Transform destinationPoint;

    private bool hasBeenInitialized;

    private void Update()
    {
        if (!hasBeenInitialized)
        {
            return;
        }

        MoveToDestination();
    }

    public void Initialize(
        Transform newStartPoint,
        Transform newDestinationPoint)
    {
        if (newStartPoint == null ||
            newDestinationPoint == null)
        {
            Debug.LogError(
                $"{name} received invalid patrol points.",
                this
            );

            return;
        }

        startPoint = newStartPoint;
        destinationPoint = newDestinationPoint;

        transform.position = startPoint.position;

        hasBeenInitialized = true;
    }

    private void MoveToDestination()
    {
        Vector3 direction =
            destinationPoint.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                );

            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                destinationPoint.position,
                movementSpeed * Time.deltaTime
            );

        float distanceToDestination =
            Vector3.Distance(
                transform.position,
                destinationPoint.position
            );

        if (distanceToDestination <= stoppingDistance)
        {
            Destroy(gameObject);
        }
    }
}