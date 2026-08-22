using System.Collections;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesHeldText;

    private BaseResourceStorage baseResourceStorage;

    private void Awake()
    {
        Debug.Log("HUD Awake is running.", this);
    }

    private IEnumerator Start()
    {
        while (baseResourceStorage == null)
        {
            baseResourceStorage = FindAnyObjectByType<BaseResourceStorage>();
            yield return null;
        }

        Debug.Log($"HUD connected to {baseResourceStorage.gameObject.name}.");

        baseResourceStorage.ResourcesChanged += UpdateResourcesText;
        UpdateResourcesText(baseResourceStorage.StoredResources);
    }

    private void OnDestroy()
    {
        if (baseResourceStorage != null)
        {
            baseResourceStorage.ResourcesChanged -= UpdateResourcesText;
        }
    }

    private void UpdateResourcesText(int amount)
    {
        Debug.Log($"HUD received resource update from {baseResourceStorage.gameObject.name}: {amount}");

        if (resourcesHeldText != null)
        {
            resourcesHeldText.text = $"Resources: {amount}";
        }
    }
}