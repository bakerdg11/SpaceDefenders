using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField] private ShipData shipData;

    private float currentHealth;

    public ShipData ShipData => shipData;
    public float CurrentHealth => currentHealth;

    public float MaximumHealth =>
        shipData != null ? shipData.MaximumHealth : 0f;

    public float MovementSpeed =>
        shipData != null ? shipData.MovementSpeed : 0f;

    public float RotationSpeed =>
        shipData != null ? shipData.RotationSpeed : 0f;

    public float HeightOffset =>
        shipData != null ? shipData.HeightOffset : 0f;

    private void Awake()
    {
        InitializeFromData();
    }

    public void Initialize(ShipData newShipData)
    {
        shipData = newShipData;
        InitializeFromData();
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

    private void InitializeFromData()
    {
        if (shipData == null)
        {
            Debug.LogWarning(
                $"{name} does not have ShipData assigned.",
                this
            );

            return;
        }

        currentHealth = shipData.MaximumHealth;
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}