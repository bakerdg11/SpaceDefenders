using UnityEngine;

public class GridManager : MonoBehaviour
{
    [Header("Grid Size")]
    [SerializeField, Min(1)] private int width = 9;
    [SerializeField, Min(1)] private int depth = 4;

    [Header("Cell Settings")]
    [SerializeField, Min(0.1f)] private float cellSize = 2f;
    [SerializeField] private GridCell cellPrefab;

    [Tooltip(
        "Optional parent for spawned defensive ships. " +
        "Leave empty to place them at the scene root."
    )]
    [SerializeField] private Transform defensiveShipsParent;

    [Header("Base Ship")]
    [SerializeField] private GameObject baseShipPrefab;
    [SerializeField] private float baseShipHeight = 0.75f;

    [Tooltip(
        "When enabled, defensive ships cannot be placed directly " +
        "beside the base ship."
    )]
    [SerializeField] private bool baseShipBlocksAdjacentCells = true;

    [Tooltip(
        "Optional parent for the spawned base ship. " +
        "Leave empty to place it at the scene root."
    )]
    [SerializeField] private Transform baseShipParent;

    private GridCell[,] cells;

    private static readonly Vector2Int[] AdjacentDirections =
    {
        Vector2Int.right,
        Vector2Int.left,
        Vector2Int.up,
        Vector2Int.down
    };

    private void Start()
    {
        GenerateGrid();
    }

    private void GenerateGrid()
    {
        if (cellPrefab == null)
        {
            Debug.LogError(
                "Grid generation failed: Grid Cell Prefab is not assigned."
            );

            return;
        }

        cells = new GridCell[width, depth];

        float gridWidth = (width - 1) * cellSize;
        float gridDepth = (depth - 1) * cellSize;

        Vector3 bottomLeft =
            transform.position -
            new Vector3(
                gridWidth * 0.5f,
                0f,
                gridDepth * 0.5f
            );

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 cellPosition =
                    bottomLeft +
                    new Vector3(
                        x * cellSize,
                        0f,
                        z * cellSize
                    );

                GridCell newCell = Instantiate(
                    cellPrefab,
                    cellPosition,
                    Quaternion.identity,
                    transform
                );

                newCell.Initialize(x, z);
                cells[x, z] = newCell;
            }
        }

        SetupBaseShip();
    }

    private void SetupBaseShip()
    {
        if (baseShipPrefab == null)
        {
            Debug.LogWarning(
                "The grid was generated, but no Base Ship Prefab is assigned."
            );

            return;
        }

        int middleColumn = width / 2;
        int bottomRow = 0;

        GridCell baseCell = GetCell(middleColumn, bottomRow);

        if (baseCell == null)
        {
            Debug.LogError("The base ship cell could not be found.");
            return;
        }

        baseCell.SetCellType(GridCellType.BaseShip);

        bool wasPlaced = baseCell.PlaceReservedShip(
            baseShipPrefab,
            baseShipHeight,
            baseShipParent
        );

        if (wasPlaced)
        {
            Debug.Log(
                $"Base ship placed at grid position " +
                $"{baseCell.GridPosition}."
            );
        }
    }

    /// <summary>
    /// Returns the cell at the supplied grid coordinates.
    /// Returns null when the coordinates are outside the grid.
    /// </summary>
    public GridCell GetCell(int x, int z)
    {
        if (cells == null)
        {
            return null;
        }

        if (x < 0 || x >= width || z < 0 || z >= depth)
        {
            return null;
        }

        return cells[x, z];
    }

    /// <summary>
    /// Checks the selected cell and its four direct neighbours.
    /// Diagonal cells are not checked.
    /// </summary>
    public bool CanPlaceShipAt(int x, int z)
    {
        GridCell targetCell = GetCell(x, z);

        if (targetCell == null)
        {
            return false;
        }

        if (targetCell.CellType != GridCellType.Buildable)
        {
            return false;
        }

        if (targetCell.IsOccupied)
        {
            return false;
        }

        foreach (Vector2Int direction in AdjacentDirections)
        {
            int adjacentX = x + direction.x;
            int adjacentZ = z + direction.y;

            GridCell adjacentCell = GetCell(
                adjacentX,
                adjacentZ
            );

            // A null cell means that direction is outside the grid.
            if (adjacentCell == null)
            {
                continue;
            }

            if (!adjacentCell.IsOccupied)
            {
                continue;
            }

            // Optionally allow placement beside the player's base.
            if (!baseShipBlocksAdjacentCells &&
                adjacentCell.CellType == GridCellType.BaseShip)
            {
                continue;
            }

            return false;
        }

        return true;
    }

    /// <summary>
    /// Attempts to place a defensive ship after checking the
    /// target cell and all four adjacent cells.
    /// </summary>
    public bool TryPlaceShipAt(int x, int z, ShipData shipData)
    {
        if (shipData == null || shipData.ShipPrefab == null)
        {
            Debug.LogWarning("Cannot place ship: ShipData is invalid.");
            return false;
        }

        if (!CanPlaceShipAt(x, z))
        {
            Debug.Log(
                $"Cannot place ship at ({x}, {z}). " +
                "The cell is unavailable or has an occupied neighbour."
            );

            return false;
        }

        GridCell targetCell = GetCell(x, z);

        return targetCell.TryPlaceShip(
            shipData,
            defensiveShipsParent
        );
    }

    /// <summary>
    /// Convenience overload that accepts a GridCell directly.
    /// Useful when the cell was found using a raycast.
    /// </summary>
    public bool TryPlaceShipAt(
        GridCell targetCell,
        ShipData shipData)
    {
        if (targetCell == null)
        {
            return false;
        }

        Vector2Int position = targetCell.GridPosition;

        return TryPlaceShipAt(
            position.x,
            position.y,
            shipData
        );
    }

    /// <summary>
    /// Removes a defensive ship from a cell.
    /// The base ship cannot be removed using this method.
    /// </summary>
    public bool TryRemoveShipAt(int x, int z)
    {
        GridCell targetCell = GetCell(x, z);

        if (targetCell == null ||
            targetCell.CellType != GridCellType.Buildable ||
            !targetCell.IsOccupied)
        {
            return false;
        }

        targetCell.ClearCell();
        return true;
    }
}