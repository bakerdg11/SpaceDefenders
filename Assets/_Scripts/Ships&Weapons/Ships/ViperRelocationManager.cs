using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ViperRelocationManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ShipPlacementManager shipPlacementManager;
    [SerializeField] private Camera inputCamera;

    [Header("Raycast")]
    [SerializeField] private LayerMask selectableLayers;

    [SerializeField, Min(1f)]
    private float raycastDistance = 500f;

    private ViperShip selectedViper;

    public ViperShip SelectedViper => selectedViper;
    public bool IsMovingViper => selectedViper != null;

    private void Awake()
    {
        if (inputCamera == null)
        {
            inputCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (Pointer.current == null)
        {
            return;
        }

        if (!Pointer.current.press.wasPressedThisFrame)
        {
            return;
        }

        /*
         * Do not relocate ships while the normal placement system
         * is waiting for the player to place a newly purchased ship.
         */
        if (shipPlacementManager != null &&
            shipPlacementManager.IsPlacingShip)
        {
            return;
        }

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        CheckSelection();
    }

    private void CheckSelection()
    {
        if (inputCamera == null ||
            gridManager == null)
        {
            return;
        }

        Vector2 screenPosition =
            Pointer.current.position.ReadValue();

        Ray ray =
            inputCamera.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                raycastDistance,
                selectableLayers))
        {
            return;
        }

        ViperShip clickedViper =
            hit.collider.GetComponentInParent<ViperShip>();

        if (clickedViper != null)
        {
            SelectViper(clickedViper);
            return;
        }

        if (selectedViper == null)
        {
            return;
        }

        GridCell clickedCell =
            hit.collider.GetComponentInParent<GridCell>();

        if (clickedCell != null)
        {
            TryMoveSelectedViper(clickedCell);
        }
    }

    private void SelectViper(ViperShip viper)
    {
        if (selectedViper == viper)
        {
            selectedViper = null;
            Debug.Log("Viper movement cancelled.");
            return;
        }

        selectedViper = viper;

        Debug.Log(
            $"{selectedViper.name} selected. " +
            "Choose an available grid tile."
        );
    }

    private void TryMoveSelectedViper(
        GridCell targetCell)
    {
        bool movedSuccessfully =
            gridManager.TryMoveViper(
                selectedViper,
                targetCell
            );

        if (!movedSuccessfully)
        {
            Debug.Log(
                "That cell is not available for relocation."
            );

            return;
        }

        selectedViper = null;
    }

    public void CancelMovement()
    {
        selectedViper = null;
    }
}