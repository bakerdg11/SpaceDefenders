using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ShipPlacementManager : MonoBehaviour
{
    [Header("Required References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Camera placementCamera;
    [SerializeField] private GameObject shipSelectionPanel;

    [Header("Raycast")]
    [SerializeField] private LayerMask gridCellLayer;
    [SerializeField, Min(1f)] private float raycastDistance = 500f;

    private ShipData selectedShip;

    public ShipData SelectedShip => selectedShip;
    public bool IsPlacingShip => selectedShip != null;

    private void Awake()
    {
        if (placementCamera != null)
        {
            placementCamera = Camera.main;
        }

        if (shipSelectionPanel != null)
        {
            shipSelectionPanel.SetActive(false);
        }
    }


    private void Update()
    {
        if (!IsPlacingShip)
        {
            return;
        }

        CheckForCellSelection();
    }

    // Called by the main Place Ships button
    public void OpenShipSelectionMenu()
    {
        selectedShip = null;

        if (shipSelectionPanel != null)
        {
            shipSelectionPanel.SetActive(true);
        }
    }

    // Called when one of the ship buttons is pressed
    public void SelectShip(ShipData shipData)
    {
        if (shipData == null)
        {
            Debug.LogWarning("Cannot select ship because ShipData is null");
            return;
        }

        selectedShip = shipData;

        if (shipSelectionPanel != null)
        {
            shipSelectionPanel.SetActive(false);
        }

        Debug.Log($"Selected {selectedShip.ShipName}. " + "Select a grid tile to place it.");
    }


    // Cancels both menu selection and active placement.
    public void CancelPlacement()
    {
        selectedShip = null;

        if (shipSelectionPanel != null)
        {
            shipSelectionPanel.SetActive(false);
        }

        Debug.Log("Ship placement cancelled.");
    }


    private void CheckForCellSelection()
    {
        if (Pointer.current == null)
        {
            return;
        }

        if (!Pointer.current.press.wasPressedThisFrame)
        {
            return;
        }

        // Prevent clicks on buttons from also selecting a grid cell. 
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (placementCamera == null)
        {
            Debug.LogError("No placement camera has been assigned.");
            return;
        }

        Vector2 screenPosition = Pointer.current.position.ReadValue();

        Ray ray = placementCamera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, raycastDistance, gridCellLayer))
        {
            return;
        }

        GridCell selectedCell = hit.collider.GetComponentInParent<GridCell>();

        if (selectedCell == null)
        {
            return;
        }

        TryPlaceSelectedShip(selectedCell);
    }


    private void TryPlaceSelectedShip(GridCell selectedCell)
    {
        if (selectedShip == null)
        {
            return;
        }

        bool placementSucceeded = gridManager.TryPlaceShipAt(selectedCell, selectedShip);

        if (!placementSucceeded)
        {
            Debug.Log($"Could not place {selectedShip.ShipName} on " + $"{selectedCell.GridPosition}. The tile may be occupied, " + "reserved or directly beside another ship.");
            return;
        }

        Debug.Log($"Placed {selectedShip.ShipName} on " + $"{selectedCell.GridPosition}.");

        selectedShip = null;
    }



}
