using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private EnemyShipData shipData;
    private float currentHealth;
    private bool isDead;

    [Header("Resource Drop")]
    [SerializeField] private ResourcePickup resourcePickupPrefab;
    [SerializeField] private float resourceDropHeightOffset = 0.5f;

    public float CurrentHealth => currentHealth;
    public float MaximumHealth => shipData != null ? shipData.MaximumHealth : 0f;

    public void Initialize(EnemyShipData newShipData)
    {
        if (newShipData == null)
        {
            Debug.LogError($"{name} received invalid EnemyShipData.", this);
            return;
        }

        shipData = newShipData;
        currentHealth = shipData.MaximumHealth;
        isDead = false;
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || isDead)
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

        if (shipData == null)
        {
            Debug.LogWarning($"{name} has no EnemyShipData assigned.", this);
            return;
        }

        Vector3 dropPosition = transform.position + Vector3.up * resourceDropHeightOffset;

        ResourcePickup spawnedPickup = Instantiate(resourcePickupPrefab, dropPosition, Quaternion.identity);
        spawnedPickup.SetResourceAmount(shipData.ResourceDropValue);
    }
}