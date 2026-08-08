using UnityEngine;
using System.Collections;

public class ShipController : MonoBehaviour
{
    [SerializeField] private ShipData shipData;
    private float currentHealth;
    [SerializeField] private float activationDelay = 3f;
    private bool isOperational;

    public ShipData ShipData => shipData;
    public float CurrentHealth => currentHealth;
    public float ActivationDelay => activationDelay;
    public bool IsOperational => isOperational;


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
        StartCoroutine(ActivationCountdown());
    }

    private IEnumerator ActivationCountdown()
    {
        isOperational = false;

        yield return new WaitForSeconds(activationDelay);

        isOperational = true;
    }

    public void ActivateImmediately()
    {
        isOperational = true;
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
        Destroy(gameObject);
    }
}