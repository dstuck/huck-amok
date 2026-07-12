using UnityEngine;

public static class SlimeShootPattern
{
    public static void Fire(EnemyContext context, EnemyConfig config, SlimeComposition composition)
    {
        if (context == null || config == null || config.projectilePrefab == null)
            return;

        var stats = composition != null
            ? composition.BuildAttackStats()
            : SlimeAttackStats.Default;

        Vector2 baseDirection = context.DirectionToPlayer;
        if (baseDirection.sqrMagnitude <= 0.0001f)
            baseDirection = Vector2.right;

        float[] angles = GetSpreadAngles(stats.ProjectileCount);

        for (int i = 0; i < angles.Length; i++)
        {
            Vector2 direction = Rotate(baseDirection, angles[i]);
            context.SpawnProjectile(direction, config, stats, composition);
        }

        if (config.shootSounds != null && config.shootSounds.Length > 0 && SoundFXManager.Instance != null)
        {
            SoundFXManager.Instance.PlayRandomSoundFXClip(
                config.shootSounds,
                context.Transform,
                category: SfxCategory.Shoot);
        }
    }

    private static float[] GetSpreadAngles(int projectileCount)
    {
        if (projectileCount <= 1)
            return new[] { 0f };

        float totalSpread = SlimeAttackStats.MultiShotSpreadDegrees * 2f;
        float start = -SlimeAttackStats.MultiShotSpreadDegrees;
        float step = totalSpread / (projectileCount - 1);

        var angles = new float[projectileCount];
        for (int i = 0; i < projectileCount; i++)
            angles[i] = start + step * i;

        return angles;
    }

    private static Vector2 Rotate(Vector2 direction, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);
        return new Vector2(
            direction.x * cos - direction.y * sin,
            direction.x * sin + direction.y * cos);
    }
}
