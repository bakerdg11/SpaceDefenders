using UnityEngine;

[RequireComponent(typeof(ShipController))]
[RequireComponent(typeof(ShipWeaponController))]
public class TankShip : MonoBehaviour
{
    private ShipController shipController;
    private ShipWeaponController weaponController;

    public ShipController ShipController => shipController;
    public ShipWeaponController WeaponController => weaponController;

    private void Awake()
    {
        shipController = GetComponent<ShipController>();
        weaponController = GetComponent<ShipWeaponController>();

        if (shipController == null)
        {
            Debug.LogError(
                $"{name} requires a ShipController component.",
                this
            );
        }

        if (weaponController == null)
        {
            Debug.LogError(
                $"{name} requires a ShipWeaponController component.",
                this
            );
        }
    }
}