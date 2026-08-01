using UnityEngine;
using UnityEngine.UI;

public class ShipSelectionManager : MonoBehaviour
{
    [Header("Ship Selection")]
    [SerializeField] private ShipData shipData;
    [SerializeField] private ShipPlacementManager placementManager;

    [Header("Optional")]
    [SerializeField] private Button button;

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
    }

    public void SelectShip()
    {
        if (placementManager == null)
        {
            Debug.LogError($"{name} does not have a ShipPlacementManager assigned.", this);
            return;
        }

        if (shipData == null)
        {
            Debug.LogError($"{name} does not have ShipData assigned.", this);
            return;
        }

        placementManager.SelectShip(shipData);

    }





}
