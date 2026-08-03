using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Points")]
    [SerializeField] private Transform point1;
    [SerializeField] private Transform point2;

    [Header("Movement")]
    [SerializeField, Min(0.1f)] private float movementSpeed = 3f;
    [SerializeField, Min(0.01f)] private float stoppingDistance = 0.1f;
    [SerializeField, Min(0f)] private float rotationSpeed = 360f;

    private Transform currentTarget;

    private void Start()
    {
        if (point1 == null || point2 == null)
        {
            Debug.LogError(
                $"{name} is missing patrol points.",
                this
            );

            enabled = false;
            return;
        }

        currentTarget = point2;
    }

    private void Update()
    {
        MoveTowardsTarget();
    }

    private void MoveTowardsTarget()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            currentTarget.position,
            movementSpeed * Time.deltaTime
        );

        Vector3 direction =
            currentTarget.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction);

            transform.rotation =
                Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
        }

        float distance = Vector3.Distance(
            transform.position,
            currentTarget.position
        );

        if (distance <= stoppingDistance)
        {
            currentTarget =
                currentTarget == point1
                ? point2
                : point1;
        }
    }
}