using UnityEngine;

public static class SlimeSpawnHelper
{
    public static GameObject Spawn(
        GameObject prefab,
        Vector2 position,
        SlimeType[] compositionSlots = null,
        bool beginInvulnerability = false)
    {
        if (prefab == null)
            return null;

        var instance = Object.Instantiate(prefab, position, Quaternion.identity);
        ApplyComposition(instance, compositionSlots);

        if (beginInvulnerability && instance.TryGetComponent<InvulnerabilityController>(out var invulnerability))
            invulnerability.BeginInvulnerability();

        return instance;
    }

    public static GameObject Spawn(
        GameObject prefab,
        Vector2 position,
        SlimeComposition composition,
        bool beginInvulnerability = false)
    {
        return Spawn(
            prefab,
            position,
            composition != null ? composition.Slots : null,
            beginInvulnerability);
    }

    public static void ApplyComposition(GameObject instance, SlimeType[] compositionSlots)
    {
        if (instance == null)
            return;

        if (!instance.TryGetComponent<SlimeComposition>(out var targetComposition))
            targetComposition = instance.AddComponent<SlimeComposition>();

        if (compositionSlots != null)
            targetComposition.CopyFromSlots(compositionSlots);

        if (instance.TryGetComponent<SlimeVisuals>(out var visuals))
            visuals.Apply(targetComposition);
    }
}
