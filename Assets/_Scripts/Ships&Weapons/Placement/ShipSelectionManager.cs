using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShipSelectionButton : MonoBehaviour
{
    [Header("Ship Selection")]
    [SerializeField] private ShipData shipData;
    [SerializeField] private ShipPlacementManager placementManager;

    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text costText;

    public ShipData ShipData => shipData;

    private void Reset()
    {
        button = GetComponent<Button>();
    }

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        UpdateCostText();
    }

    private void OnValidate()
    {
        UpdateCostText();
    }

    public void SelectShip()
    {
        if (placementManager == null)
        {
            Debug.LogError(
                $"{name} does not have a ShipPlacementManager assigned.",
                this
            );

            return;
        }

        if (shipData == null)
        {
            Debug.LogError(
                $"{name} does not have ShipData assigned.",
                this
            );

            return;
        }

        placementManager.SelectShip(shipData);
    }

    private void UpdateCostText()
    {
        if (costText == null)
        {
            return;
        }

        costText.text = shipData != null
            ? $"Cost: {shipData.ShipPlacementCost}"
            : "Cost: --";
    }
}