using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    [SerializeField, Min(1)] private int resourceAmount = 10;

    public int ResourceAmount => resourceAmount;
    public bool IsReserved { get; private set; }

    private void OnEnable()
    {
        ResourceManager.RegisterResource(this);
    }

    private void OnDisable()
    {
        ResourceManager.UnregisterResource(this);
    }

    public bool TryReserve()
    {
        if (IsReserved)
        {
            return false;
        }

        IsReserved = true;
        return true;
    }

    public void ReleaseReservation()
    {
        IsReserved = false;
    }

    public int Collect()
    {
        int amountCollected = resourceAmount;

        ResourceManager.UnregisterResource(this);
        Destroy(gameObject);

        return amountCollected;
    }
}