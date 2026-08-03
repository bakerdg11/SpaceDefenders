using Unity.VisualScripting;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float maximumHealth = 100f;
    private float currentHealth;
    private bool isDead;
    public float CurrentHealth => currentHealth;
    public float MaximumHealth => maximumHealth;

    [Header("Resource Drop")]
    [SerializeField] private ResourcePickup resourcePickupPrefab;
    [SerializeField] private float resourceDropHeightOffset = 0.5f;


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

        Debug.Log(
            $"{name} took {damage} damage. " +
            $"Health: {currentHealth}/{maximumHealth}"
        );

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

        DropResource();

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
        Instantiate(resourcePickupPrefab, dropPosition, Quaternion.identity);

    }


}