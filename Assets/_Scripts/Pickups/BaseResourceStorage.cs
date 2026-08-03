using System;
using UnityEngine;

public class BaseResourceStorage : MonoBehaviour
{
    [SerializeField, Min(0)] private int storedResources;

    public int StoredResources => storedResources;

    public event Action<int> ResourcesChanged;

    private void Start()
    {
        ResourcesChanged?.Invoke(storedResources);
        storedResources = 100;
    }

    public void DepositResources(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        storedResources += amount;

        ResourcesChanged?.Invoke(storedResources);

        Debug.Log(
            $"Deposited {amount} resources. " +
            $"Base now has {storedResources}."
        );
    }

    public bool CanAfford(int cost)
    {
        return cost >= 0 && storedResources >= cost;
    }

    public bool TrySpendResources(int cost)
    {
        if (cost < 0)
        {
            Debug.LogWarning("Resource cost cannot be negative.");
            return false;
        }

        if (!CanAfford(cost))
        {
            return false;
        }

        storedResources -= cost;
        ResourcesChanged?.Invoke(storedResources);

        Debug.Log(
            $"Spent {cost} resources. " +
            $"Base now has {storedResources}."
        );

        return true;
    }


}