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
}