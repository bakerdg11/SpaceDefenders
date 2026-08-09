using UnityEngine;
using System;
using System.Collections;

public class ShipController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipData shipData;

    [Header("Placement")]
    [SerializeField] private float activationDelay = 3f;
    private bool isOperational;

    [Header("Ship Health")]
    private float currentHealth;
    public event Action<float, float> HealthChanged;


    public ShipData ShipData => shipData;

    public float ActivationDelay => activationDelay;
    public bool IsOperational => isOperational;

    public float CurrentHealth => currentHealth;


    // Ship Data Initialization
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
        HealthChanged?.Invoke(currentHealth, shipData.MaximumHealth);
    }

    public void TakeDamage(float damage)
    {
        if (damage <= 0f || currentHealth <= 0f)
        {
            return;
        }

        currentHealth = Mathf.Max(0f, currentHealth - damage);
        HealthChanged?.Invoke(currentHealth, shipData.MaximumHealth);

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