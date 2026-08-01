using UnityEngine;

public class BaseResourceStorage : MonoBehaviour
{
    [SerializeField, Min(0)] private int storedResources;

    public int StoredResources => storedResources;

    public void DepositResources(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        storedResources += amount;

        Debug.Log(
            $"Deposited {amount} resources. " +
            $"Base now has {storedResources}."
        );
    }
}