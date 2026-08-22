using System;
using UnityEngine;

public class BaseResourceStorage : MonoBehaviour
{
    [SerializeField] private int storedResources = 100;

    public int StoredResources => storedResources;

    public event Action<int> ResourcesChanged;

    public void DepositResources(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        storedResources += amount;
        Debug.Log($"Deposited resources on {gameObject.name}. New total: {storedResources}");
        ResourcesChanged?.Invoke(storedResources);
    }

    public bool CanAfford(int cost)
    {
        return cost >= 0 && storedResources >= cost;
    }

    public bool TrySpendResources(int cost)
    {
        if (cost < 0)
        {
            return false;
        }

        if (!CanAfford(cost))
        {
            return false;
        }

        storedResources -= cost;
        Debug.Log($"Spent resources on {gameObject.name}. New total: {storedResources}");
        ResourcesChanged?.Invoke(storedResources);

        return true;
    }
}