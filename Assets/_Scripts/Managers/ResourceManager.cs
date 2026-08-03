using System.Collections.Generic;
using UnityEngine;

public static class ResourceManager
{
    private static readonly List<ResourcePickup> availableResources = new();

    public static void RegisterResource(ResourcePickup resource)
    {
        if (resource == null || availableResources.Contains(resource))
        {
            return;
        }

        availableResources.Add(resource);
    }

    public static void UnregisterResource(ResourcePickup resource)
    {
        availableResources.Remove(resource);
    }

    public static ResourcePickup GetAvailableResource()
    {
        availableResources.RemoveAll(resource => resource == null);

        foreach (ResourcePickup resource in availableResources)
        {
            if (!resource.IsReserved)
            {
                return resource;
            }
        }

        return null;
    }

    public static bool HasAvailableResource()
    {
        availableResources.RemoveAll(resource => resource == null);

        foreach (ResourcePickup resource in availableResources)
        {
            if (!resource.IsReserved)
            {
                return true;
            }
        }

        return false;
    }



}