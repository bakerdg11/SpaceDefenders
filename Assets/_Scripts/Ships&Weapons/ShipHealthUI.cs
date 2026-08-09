using UnityEngine;
using UnityEngine.UI;

public class ShipHealthUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Slider ammoEnergySlider;

    [Header("Visibility")]
    [SerializeField] private float visibilityThreshold = 0.2f;
    [SerializeField] private bool useEnergyInsteadOfAmmo;

    private ShipController shipController;
    private ShipWeaponController weaponController;

    private bool energyWarningActive;

    private void Start()
    {
        shipController = GetComponentInParent<ShipController>();
        weaponController = GetComponentInParent<ShipWeaponController>();

        if (shipController != null)
        {
            shipController.HealthChanged += UpdateHealth;
            UpdateHealth(shipController.CurrentHealth, shipController.MaximumHealth);
        }

        if (weaponController == null)
        {
            return;
        }

        if (useEnergyInsteadOfAmmo)
        {
            weaponController.EnergyChanged += UpdateEnergy;

            if (weaponController.EquippedWeapon != null)
            {
                UpdateEnergy(weaponController.CurrentEnergy, weaponController.EquippedWeapon.MaximumEnergy);
            }
        }
        else
        {
            weaponController.AmmunitionChanged += UpdateAmmo;

            if (weaponController.EquippedWeapon != null)
            {
                UpdateAmmo(weaponController.CurrentAmmunition, weaponController.EquippedWeapon.Ammunition);
            }
        }
    }

    private void UpdateHealth(float currentHealth, float maximumHealth)
    {
        float percentage = maximumHealth > 0f ? currentHealth / maximumHealth : 0f;

        healthSlider.value = percentage;
        healthSlider.gameObject.SetActive(percentage <= visibilityThreshold);
    }

    private void UpdateAmmo(int currentAmmo, int maximumAmmo)
    {
        float percentage = maximumAmmo > 0 ? (float)currentAmmo / maximumAmmo : 0f;

        ammoEnergySlider.value = percentage;
        ammoEnergySlider.gameObject.SetActive(percentage <= visibilityThreshold);
    }

    private void UpdateEnergy(float currentEnergy, float maximumEnergy)
    {
        float percentage = maximumEnergy > 0f ? currentEnergy / maximumEnergy : 0f;

        ammoEnergySlider.value = percentage;

        if (!energyWarningActive && percentage <= visibilityThreshold)
        {
            energyWarningActive = true;
            Debug.Log("Energy bar turned ON.");
        }

        if (energyWarningActive && percentage >= 1f)
        {
            energyWarningActive = false;
            Debug.Log("Energy bar turned OFF.");
        }

        ammoEnergySlider.gameObject.SetActive(energyWarningActive);
    }

    private void OnDestroy()
    {
        if (shipController != null)
        {
            shipController.HealthChanged -= UpdateHealth;
        }

        if (weaponController == null)
        {
            return;
        }

        weaponController.AmmunitionChanged -= UpdateAmmo;
        weaponController.EnergyChanged -= UpdateEnergy;
    }
}