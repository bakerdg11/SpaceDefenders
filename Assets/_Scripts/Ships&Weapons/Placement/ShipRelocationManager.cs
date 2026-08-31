using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ShipRelocationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera mainCamera;

    [Header("Raycast")]
    [SerializeField] private LayerMask shipLayer;
    [SerializeField] private LayerMask gridCellLayer;

    private GameObject selectedShip;
    private GridCell sourceCell;
    private bool isMovingShip;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<GridManager>();
        }
    }

    private void Update()
    {
        if (isMovingShip)
        {
            return;
        }

        if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame)
        {
            return;
        }

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (selectedShip == null)
        {
            TrySelectShip();
        }
        else
        {
            TryRelocateSelectedShip();
        }
    }

    private void TrySelectShip()
    {
        if (mainCamera == null)
        {
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Pointer.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, shipLayer))
        {
            return;
        }

        ShipController shipController = hit.collider.GetComponentInParent<ShipController>();

        if (shipController == null)
        {
            return;
        }

        if (!CanRelocateShip(shipController.gameObject))
        {
            return;
        }

        GridCell occupiedCell = FindCellContainingShip(shipController.gameObject);

        if (occupiedCell == null)
        {
            Debug.LogWarning($"{shipController.name} is not assigned to a GridCell.", shipController);
            return;
        }

        selectedShip = shipController.gameObject;
        sourceCell = occupiedCell;

        Debug.Log($"Selected {selectedShip.name} for relocation.");
    }

    private void TryRelocateSelectedShip()
    {
        if (mainCamera == null)
        {
            ClearSelection();
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(Pointer.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, gridCellLayer))
        {
            ClearSelection();
            return;
        }

        GridCell targetCell = hit.collider.GetComponentInParent<GridCell>();

        if (targetCell == null)
        {
            ClearSelection();
            return;
        }

        if (targetCell == sourceCell)
        {
            ClearSelection();
            return;
        }

        ShipController shipController = selectedShip.GetComponent<ShipController>();

        if (shipController == null || shipController.ShipData == null)
        {
            ClearSelection();
            return;
        }

        if (!gridManager.CanMoveShipTo(sourceCell, targetCell))
        {
            Debug.Log($"Cannot move {selectedShip.name} to {targetCell.GridPosition}.");
            ClearSelection();
            return;
        }

        GameObject releasedShip = sourceCell.ReleasePlacedShip();

        if (releasedShip == null)
        {
            ClearSelection();
            return;
        }

        bool assignedSuccessfully = targetCell.AssignExistingShip(releasedShip, shipController.HeightOffset, false);

        if (!assignedSuccessfully)
        {
            sourceCell.AssignExistingShip(releasedShip, shipController.HeightOffset);
            Debug.LogWarning($"{selectedShip.name} could not be moved to {targetCell.GridPosition}.", selectedShip);
            ClearSelection();
            return;
        }

        StartCoroutine(MoveShipToCell(releasedShip, targetCell, shipController));
    }

    private IEnumerator MoveShipToCell(GameObject ship, GridCell targetCell, ShipController shipController)
    {
        isMovingShip = true;

        Vector3 destination = targetCell.transform.position + Vector3.up * shipController.HeightOffset;

        while (ship != null && Vector3.Distance(ship.transform.position, destination) > 0.05f)
        {
            Vector3 direction = destination - ship.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                ship.transform.rotation = Quaternion.RotateTowards(ship.transform.rotation, targetRotation, shipController.RotationSpeed * Time.deltaTime);
            }

            ship.transform.position = Vector3.MoveTowards(ship.transform.position, destination, shipController.MovementSpeed * Time.deltaTime);

            yield return null;
        }

        if (ship != null)
        {
            ship.transform.position = destination;
        }

        Debug.Log($"Moved {ship?.name} to {targetCell.GridPosition}.");

        isMovingShip = false;
        ClearSelection();
    }

    private bool CanRelocateShip(GameObject ship)
    {
        if (ship.GetComponent<ViperShip>() != null)
        {
            return true;
        }

        if (ship.GetComponent<CollectorShip>() != null)
        {
            return true;
        }

        if (ship.GetComponent<TankShip>() != null)
        {
            return true;
        }

        return false;
    }

    private GridCell FindCellContainingShip(GameObject ship)
    {
        GridCell[] cells = FindObjectsByType<GridCell>();

        foreach (GridCell cell in cells)
        {
            if (cell.PlacedShip == ship)
            {
                return cell;
            }
        }

        return null;
    }

    private void ClearSelection()
    {
        selectedShip = null;
        sourceCell = null;
    }
}