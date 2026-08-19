using TMPro;
using UnityEngine;

public class ShipPlacementCountUI : MonoBehaviour
{
    [SerializeField] private ShipData shipData;
    [SerializeField] private TMP_Text currentPlacedText;
    [SerializeField] private TMP_Text maximumPlacedText;

    [SerializeField] private ShipPlacementManager shipPlacementManager;

    private void Start()
    {
        if (shipPlacementManager == null)
        {
            Debug.LogError($"{name} does not have a ShipPlacementManager assigned.", this);
            return;
        }

        shipPlacementManager.ShipCountChanged += OnShipCountChanged;
        RefreshText();
    }

    private void OnShipCountChanged(ShipData changedShipData)
    {
        if (changedShipData != shipData)
        {
            return;
        }

        RefreshText();
    }

    private void RefreshText()
    {
        if (shipData == null || shipPlacementManager == null)
        {
            return;
        }

        if (currentPlacedText != null)
        {
            currentPlacedText.text = shipPlacementManager.GetPlacedCount(shipData).ToString();
        }

        if (maximumPlacedText != null)
        {
            maximumPlacedText.text = shipData.MaximumPlaced.ToString();
        }
    }

    private void OnDestroy()
    {
        if (shipPlacementManager != null)
        {
            shipPlacementManager.ShipCountChanged -= OnShipCountChanged;
        }
    }
}