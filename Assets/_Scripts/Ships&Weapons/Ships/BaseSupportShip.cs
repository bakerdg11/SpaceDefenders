using UnityEngine;

[RequireComponent(typeof(ShipController))]
[RequireComponent(typeof(ShipWeaponController))]
public class BaseSupportShip : MonoBehaviour
{
    private ShipController shipController;
    private ShipWeaponController weaponController;

    public ShipController ShipController => shipController;
    public ShipWeaponController WeaponController => weaponController;

    private void Awake()
    {
        shipController = GetComponent<ShipController>();
        weaponController = GetComponent<ShipWeaponController>();
    }
}