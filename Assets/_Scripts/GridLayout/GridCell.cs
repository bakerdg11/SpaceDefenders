using UnityEngine;

public enum GridCellType
{
    Buildable,
    BaseShip,
    Blocked
}

public class GridCell : MonoBehaviour
{
    public Vector2Int GridPosition { get; private set; }
    public bool IsOccupied => placedShip != null;
    public GridCellType CellType { get; private set; } = GridCellType.Buildable;
    public GameObject PlacedShip => placedShip;

    private GameObject placedShip;

    public void Initialize(int x, int z)
    {
        GridPosition = new Vector2Int(x, z);
        name = $"GridCell_{x}_{z}";
    }

    public void SetCellType(GridCellType newType)
    {
        CellType = newType;
    }

    public bool TryPlaceShip(ShipData shipData, Transform shipParent = null)
    {
        if (CellType != GridCellType.Buildable)
        {
            return false;
        }

        if (IsOccupied)
        {
            return false;
        }

        if (shipData == null)
        {
            Debug.LogWarning($"Cannot place a ship on {name}: ShipData is null.", this);
            return false;
        }

        if (shipData.ShipPrefab == null)
        {
            Debug.LogWarning($"Cannot place {shipData.ShipName} on {name}: its ShipData does not have a Ship Prefab assigned.", shipData);
            return false;
        }

        Vector3 shipPosition = transform.position + Vector3.up * shipData.HeightOffset;

        placedShip = Instantiate(shipData.ShipPrefab, shipPosition, Quaternion.identity, shipParent);

        ShipController shipController = placedShip.GetComponent<ShipController>();

        if (shipController != null)
        {
            shipController.Initialize(shipData);
        }
        else
        {
            Debug.LogWarning($"{placedShip.name} does not contain a ShipController.", placedShip);
        }

        CollectorShip collectorShip = placedShip.GetComponent<CollectorShip>();

        if (collectorShip != null)
        {
            collectorShip.InitializeCollector(shipPosition);
        }

        ViperShip viperShip = placedShip.GetComponent<ViperShip>();

        if (viperShip != null)
        {
            viperShip.InitializeViper(this);
        }

        return true;
    }

    public bool PlaceReservedShip(GameObject shipPrefab, float heightOffset, Transform shipParent = null)
    {
        if (IsOccupied)
        {
            Debug.LogWarning($"Cannot place the reserved ship on {name}. The cell is already occupied.", this);
            return false;
        }

        if (shipPrefab == null)
        {
            Debug.LogWarning($"No reserved ship prefab was provided for {name}.", this);
            return false;
        }

        placedShip = SpawnShip(shipPrefab, heightOffset, shipParent);

        if (placedShip == null)
        {
            return false;
        }

        ShipController shipController = placedShip.GetComponent<ShipController>();

        if (shipController != null)
        {
            shipController.ActivateImmediately();
        }
        else
        {
            Debug.LogWarning($"{placedShip.name} does not contain a ShipController.", placedShip);
        }

        return true;
    }

    public GameObject ReleasePlacedShip()
    {
        GameObject releasedShip = placedShip;
        placedShip = null;

        return releasedShip;
    }

    public bool AssignExistingShip(GameObject ship, float heightOffset)
    {
        if (ship == null)
        {
            return false;
        }

        if (CellType != GridCellType.Buildable || IsOccupied)
        {
            return false;
        }

        placedShip = ship;
        placedShip.transform.position = transform.position + Vector3.up * heightOffset;

        return true;
    }

    public void ClearCell()
    {
        if (placedShip != null)
        {
            Destroy(placedShip);
        }

        placedShip = null;
    }

    private GameObject SpawnShip(GameObject shipPrefab, float heightOffset, Transform shipParent)
    {
        Vector3 shipPosition = transform.position + Vector3.up * heightOffset;

        return Instantiate(shipPrefab, shipPosition, Quaternion.identity, shipParent);
    }
}