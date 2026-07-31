using System;
using UnityEngine;

[Serializable]
public class ShipWeaponSlot
{
    [SerializeField] private string slotName = "Weapon Slot";
    [SerializeField] private WeaponType[] allowedWeaponTypes;
    [SerializeField] private WeaponData startingWeapon;

    public string SlotName => slotName;
    public WeaponType[] AllowedWeaponTypes => allowedWeaponTypes;
    public WeaponData StartingWeapon => startingWeapon;

    public bool CanEquip(WeaponData weapon)
    {
        if (weapon == null || allowedWeaponTypes == null)
        {
            return false;
        }

        foreach (WeaponType allowedType in allowedWeaponTypes)
        {
            if (allowedType == weapon.WeaponType)
            {
                return true;
            }
        }

        return false;
    }
}