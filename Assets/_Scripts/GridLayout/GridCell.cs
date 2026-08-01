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

    public GridCellType CellType { get; private set; }
        = GridCellType.Buildable;

    public GameObject PlacedShip => placedShip;

    private GameObject placedShip;

    /// <summary>
    /// Sets the cell's grid coordinates.
    /// </summary>
    public void Initialize(int x, int z)
    {
        GridPosition = new Vector2Int(x, z);
        name = $"GridCell_{x}_{z}";
    }

    /// <summary>
    /// Changes whether this cell is buildable, reserved for the base,
    /// or completely blocked.
    /// </summary>
    public void SetCellType(GridCellType newType)
    {
        CellType = newType;
    }

    /// <summary>
    /// Places a normal defensive ship on this cell.
    /// The GridManager should validate placement before calling this.
    /// </summary>
    public bool TryPlaceShip(
        ShipData shipData,
        Transform shipParent = null)
    {
        if (CellType != GridCellType.Buildable)
        {
            return false;
        }

        if (IsOccupied ||
            shipData == null ||
            shipData.ShipPrefab == null)
        {
            return false;
        }

        Vector3 shipPosition =
            transform.position +
            Vector3.up * shipData.HeightOffset;

        placedShip = Instantiate(
            shipData.ShipPrefab,
            shipPosition,
            Quaternion.identity,
            shipParent
        );

        ShipController controller =
            placedShip.GetComponent<ShipController>();

        if (controller != null)
        {
            controller.Initialize(shipData);
        }
        else
        {
            Debug.LogWarning(
                $"{placedShip.name} does not contain a ShipController.",
                placedShip
            );
        }

        CollectorShip collector =
    placedShip.GetComponent<CollectorShip>();

        if (collector != null)
        {
            collector.InitializeCollector(shipPosition);
        }

        return true;
    }

    /// <summary>
    /// Places a reserved object such as the player's base ship.
    /// This ignores the normal Buildable cell requirement.
    /// </summary>
    public bool PlaceReservedShip(
        GameObject shipPrefab,
        float heightOffset,
        Transform shipParent = null)
    {
        if (IsOccupied)
        {
            Debug.LogWarning(
                $"Cannot place the reserved ship on {name}. " +
                "The cell is already occupied."
            );

            return false;
        }

        if (shipPrefab == null)
        {
            Debug.LogWarning(
                $"No reserved ship prefab was provided for {name}."
            );

            return false;
        }

        placedShip = SpawnShip(
            shipPrefab,
            heightOffset,
            shipParent
        );

        return placedShip != null;
    }

    /// <summary>
    /// Removes the ship occupying this cell.
    /// </summary>
    public void ClearCell()
    {
        if (placedShip != null)
        {
            Destroy(placedShip);
        }

        placedShip = null;
    }

    private GameObject SpawnShip(
        GameObject shipPrefab,
        float heightOffset,
        Transform shipParent)
    {
        Vector3 shipPosition =
            transform.position + Vector3.up * heightOffset;

        return Instantiate(
            shipPrefab,
            shipPosition,
            Quaternion.identity,
            shipParent
        );
    }
}