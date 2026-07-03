using UnityEngine;

public static class SlimeSpawnHelper
{
    public static GameObject Spawn(
        GameObject prefab,
        Vector2 position,
        SlimeComposition composition = null,
        bool beginInvulnerability = false)
    {
        if (prefab == null)
            return null;

        var instance = Object.Instantiate(prefab, position, Quaternion.identity);

        if (composition != null)
        {
            if (!instance.TryGetComponent<SlimeComposition>(out var targetComposition))
                targetComposition = instance.AddComponent<SlimeComposition>();

            targetComposition.CopyFrom(composition);
        }

        if (beginInvulnerability && instance.TryGetComponent<InvulnerabilityController>(out var invulnerability))
            invulnerability.BeginInvulnerability();

        return instance;
    }
}
