using Unity.VisualScripting;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maximumHealth = 100f;

    [Header("Resource Drop")]
    [SerializeField] private ResourcePickup resourcePickupPrefab;
    [SerializeField] private float resourceDropHeightOffset = 0.5f;
    [SerializeField, Min(0)] private int resourceDropValue = 10;

    private float currentHealth;
    private bool isDead;

    public float CurrentHealth => currentHealth;
    public float MaximumHealth => maximumHealth;

    private void Awake()
    {
        currentHealth = maximumHealth;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || currentHealth <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        DropResource();

        EnemyBaseShip enemyBaseShip = GetComponent<EnemyBaseShip>();

        if (enemyBaseShip != null)
        {
            enemyBaseShip.NotifyDestroyed();
        }

        Destroy(gameObject);
    }

    private void DropResource()
    {
        if (resourcePickupPrefab == null)
        {
            Debug.LogWarning($"{name} has no Resource Pickup Prefab assigned.", this);
            return;
        }

        Vector3 dropPosition = transform.position + Vector3.up * resourceDropHeightOffset;
        ResourcePickup spawnedPickup = Instantiate(resourcePickupPrefab, dropPosition, Quaternion.identity);

        spawnedPickup.SetResourceAmount(resourceDropValue);
    }
}