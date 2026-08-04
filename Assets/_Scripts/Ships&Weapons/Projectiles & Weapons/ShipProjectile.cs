using UnityEngine;

public class ShipProjectile : MonoBehaviour
{
    [SerializeField, Min(0.01f)]
    private float hitDistance = 0.2f;

    [SerializeField, Min(0.1f)]
    private float maximumLifetime = 10f;

    private EnemyHealth target;
    private float damage;
    private float movementSpeed;

    public void Initialize(
        EnemyHealth newTarget,
        float newDamage,
        float newMovementSpeed)
    {
        target = newTarget;
        damage = newDamage;
        movementSpeed = newMovementSpeed;

        Destroy(gameObject, maximumLifetime);
    }

    private void Update()
    {
        if (target == null ||
            target.CurrentHealth <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 targetPosition =
            target.transform.position;

        Vector3 direction =
            targetPosition - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                );
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetPosition,
                movementSpeed * Time.deltaTime
            );

        float distanceToTarget =
            Vector3.Distance(
                transform.position,
                targetPosition
            );

        if (distanceToTarget <= hitDistance)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (target != null)
        {
            target.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}