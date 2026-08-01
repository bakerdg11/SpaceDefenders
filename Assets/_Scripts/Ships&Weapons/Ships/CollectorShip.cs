using UnityEngine;

public class CollectorShip : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float stoppingDistance = 0.25f;
    [SerializeField] private float flightHeight = 0.75f;

    [Header("Resource Capacity")]
    [SerializeField] private int maximumCapacity = 100;

    private CollectorState currentState;
    private ResourcePickup targetResource;
    private BaseResourceStorage baseStorage;

    private Vector3 homePosition;
    private int carriedResources;
    private bool hasBeenInitialized;

    public CollectorState CurrentState => currentState;
    public int CarriedResources => carriedResources;

    private void Start()
    {
        if (!hasBeenInitialized)
        {
            InitializeCollector(transform.position);
        }

        FindBaseShip();
    }

    private void Update()
    {
        switch (currentState)
        {
            case CollectorState.WaitingOnGrid:
                LookForResource();
                break;

            case CollectorState.MovingToResource:
                MoveToResource();
                break;

            case CollectorState.MovingToBase:
                MoveToBase();
                break;

            case CollectorState.ReturningToGrid:
                ReturnToGrid();
                break;
        }
    }

    public void InitializeCollector(Vector3 placedPosition)
    {
        homePosition = placedPosition;
        currentState = CollectorState.WaitingOnGrid;
        hasBeenInitialized = true;
    }

    private void FindBaseShip()
    {
        baseStorage = FindFirstObjectByType<BaseResourceStorage>();

        if (baseStorage == null)
        {
            Debug.LogError(
                "Collector could not find a BaseResourceStorage.",
                this
            );
        }
    }

    private void LookForResource()
    {
        ResourcePickup availableResource =
            ResourceManager.GetAvailableResource();

        if (availableResource == null)
        {
            return;
        }

        if (!availableResource.TryReserve())
        {
            return;
        }

        targetResource = availableResource;
        currentState = CollectorState.MovingToResource;
    }

    private void MoveToResource()
    {
        if (targetResource == null)
        {
            currentState = CollectorState.ReturningToGrid;
            return;
        }

        MoveTowards(targetResource.transform.position);

        float distance = Vector3.Distance(
            transform.position,
            targetResource.transform.position
        );

        if (distance <= stoppingDistance)
        {
            CollectTargetResource();
        }
    }

    private void CollectTargetResource()
    {
        if (targetResource == null)
        {
            currentState = CollectorState.ReturningToGrid;
            return;
        }

        int availableCapacity =
            maximumCapacity - carriedResources;

        if (availableCapacity <= 0)
        {
            targetResource.ReleaseReservation();
            targetResource = null;
            currentState = CollectorState.MovingToBase;
            return;
        }

        int resourceAmount = targetResource.ResourceAmount;

        // Simple first version:
        // the Collector takes the complete pickup.
        carriedResources += targetResource.Collect();

        targetResource = null;
        currentState = CollectorState.MovingToBase;
    }

    private void MoveToBase()
    {
        if (baseStorage == null)
        {
            FindBaseShip();

            if (baseStorage == null)
            {
                return;
            }
        }

        MoveTowards(baseStorage.transform.position);

        float distance = Vector3.Distance(
            transform.position,
            baseStorage.transform.position
        );

        if (distance <= stoppingDistance)
        {
            DepositResources();
        }
    }

    private void DepositResources()
    {
        if (baseStorage != null && carriedResources > 0)
        {
            baseStorage.DepositResources(carriedResources);
            carriedResources = 0;
        }

        currentState = CollectorState.ReturningToGrid;
    }

    private void ReturnToGrid()
    {
        MoveTowards(homePosition);

        float distance = Vector3.Distance(
            transform.position,
            homePosition
        );

        if (distance <= stoppingDistance)
        {
            transform.position = homePosition;
            currentState = CollectorState.WaitingOnGrid;
        }
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 destination = new Vector3(
            targetPosition.x,
            homePosition.y + flightHeight,
            targetPosition.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            movementSpeed * Time.deltaTime
        );

        Vector3 direction =
            destination - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(direction.normalized);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                8f * Time.deltaTime
            );
        }
    }

    private void OnDestroy()
    {
        if (targetResource != null)
        {
            targetResource.ReleaseReservation();
        }
    }



    private void OnTriggerEnter(Collider other)
    {
        if (currentState != CollectorState.MovingToResource)
        {
            return;
        }

        ResourcePickup resource =
            other.GetComponentInParent<ResourcePickup>();

        if (resource == null || resource != targetResource)
        {
            return;
        }

        CollectTargetResource();
    }



}