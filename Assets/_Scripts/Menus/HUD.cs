using System.Collections;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour
{
    [SerializeField] private BaseResourceStorage baseResourceStorage;
    [SerializeField] private TMP_Text resourcesHeldText;

    private IEnumerator Start()
    {
        yield return null;

        if (baseResourceStorage == null)
        {
            baseResourceStorage =
                FindAnyObjectByType<BaseResourceStorage>();
        }

        if (baseResourceStorage == null)
        {
            Debug.LogError(
                "HUD could not find BaseResourceStorage.",
                this
            );

            yield break;
        }

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
        Debug.Log($"HUD received resource update: {amount}");

        if (resourcesHeldText != null)
        {
            resourcesHeldText.text = $"Resources: {amount}";
        }
        else
        {
            Debug.LogError("HUD Resources Held Text is not assigned.", this);
        }
    }
}