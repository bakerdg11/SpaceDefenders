using UnityEngine;

[RequireComponent(typeof(ShipController))]
public class CollectorShip : MonoBehaviour
{
    [Header("Arrival Settings")]
    [SerializeField, Min(0.01f)]
    private float stoppingDistance = 0.75f;

    [Header("Resource Capacity")]
    [SerializeField, Min(1)]
    private int maximumCapacity = 100;

    private ShipController shipController;

    private CollectorState currentState;
    private ResourcePickup targetResource;
    private BaseResourceStorage baseStorage;

    private Vector3 homePosition;
    private float movementHeight;

    private int carriedResources;
    private bool hasBeenInitialized;

    public bool CanAttack => currentState == CollectorState.WaitingOnGrid && !ResourceManager.HasAvailableResource();

    public CollectorState CurrentState => currentState;
    public int CarriedResources => carriedResources;
    public int MaximumCapacity => maximumCapacity;

    private void Awake()
    {
        shipController = GetComponent<ShipController>();

        if (shipController == null)
        {
            Debug.LogError(
                $"{name} requires a ShipController component.",
                this
            );
        }
    }

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
        if (shipController == null || !shipController.IsOperational)
        {
            return;
        }

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

        /*
         * placedPosition should already include ShipData.HeightOffset
         * because GridCell uses that value when spawning the ship.
         */
        movementHeight = placedPosition.y;

        currentState = CollectorState.WaitingOnGrid;
        carriedResources = 0;
        hasBeenInitialized = true;
    }

    private void FindBaseShip()
    {
        baseStorage =
            FindAnyObjectByType<BaseResourceStorage>();

        if (baseStorage == null)
        {
            Debug.LogError(
                $"{name} could not find BaseResourceStorage.",
                this
            );
        }
    }

    private void LookForResource()
    {
        if (carriedResources >= maximumCapacity)
        {
            currentState = CollectorState.MovingToBase;
            return;
        }

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
            DecideNextStateAfterLosingResource();
            return;
        }

        MoveTowards(targetResource.transform.position);
    }

    private void CollectTargetResource()
    {
        if (targetResource == null)
        {
            DecideNextStateAfterLosingResource();
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

        float distanceToBase =
            GetHorizontalDistance(
                transform.position,
                baseStorage.transform.position
            );

        if (distanceToBase <= stoppingDistance)
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

        TryFindNextResourceOrReturnHome();
    }

    private void ReturnToGrid()
    {
        MoveTowards(homePosition);

        float distanceToHome =
            GetHorizontalDistance(
                transform.position,
                homePosition
            );

        if (distanceToHome > stoppingDistance)
        {
            return;
        }

        transform.position = homePosition;
        currentState = CollectorState.WaitingOnGrid;
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        Vector3 destination = new Vector3(
            targetPosition.x,
            movementHeight,
            targetPosition.z
        );

        transform.position = Vector3.MoveTowards(
            transform.position,
            destination,
            shipController.MovementSpeed * Time.deltaTime
        );

        RotateTowards(destination);
    }

    private void RotateTowards(Vector3 destination)
    {
        Vector3 direction =
            destination - transform.position;

        /*
         * Keep the ship level on the X/Z plane.
         */
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        Quaternion targetRotation =
            Quaternion.LookRotation(direction.normalized);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            shipController.RotationSpeed * Time.deltaTime
        );
    }

    private void DecideNextStateAfterLosingResource()
    {
        targetResource = null;

        currentState = carriedResources > 0
            ? CollectorState.MovingToBase
            : CollectorState.ReturningToGrid;
    }

    private void TryFindNextResourceOrReturnHome()
    {
        ResourcePickup availableResource =
            ResourceManager.GetAvailableResource();

        if (availableResource != null &&
            availableResource.TryReserve())
        {
            targetResource = availableResource;
            currentState = CollectorState.MovingToResource;
            return;
        }

        targetResource = null;
        currentState = CollectorState.ReturningToGrid;
    }

    private static float GetHorizontalDistance(
        Vector3 firstPosition,
        Vector3 secondPosition)
    {
        Vector2 firstXZ = new Vector2(
            firstPosition.x,
            firstPosition.z
        );

        Vector2 secondXZ = new Vector2(
            secondPosition.x,
            secondPosition.z
        );

        return Vector2.Distance(firstXZ, secondXZ);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (currentState !=
            CollectorState.MovingToResource)
        {
            return;
        }

        ResourcePickup resource =
            other.GetComponentInParent<ResourcePickup>();

        if (resource == null ||
            resource != targetResource)
        {
            return;
        }

        CollectTargetResource();
    }

    private void OnDestroy()
    {
        if (targetResource != null)
        {
            targetResource.ReleaseReservation();
        }
    }
}