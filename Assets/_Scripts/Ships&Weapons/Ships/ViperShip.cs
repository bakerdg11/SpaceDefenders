using UnityEngine;

[RequireComponent(typeof(ShipController))]
[RequireComponent(typeof(ShipWeaponController))]
public class ViperShip : MonoBehaviour
{
    public GridCell CurrentCell { get; private set; }

    public void InitializeViper(GridCell startingCell)
    {
        CurrentCell = startingCell;
    }

    public void SetCurrentCell(GridCell newCell)
    {
        CurrentCell = newCell;
    }
}