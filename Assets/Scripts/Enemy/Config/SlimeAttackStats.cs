using System;
using System.Linq;
using UnityEngine;

public readonly struct SlimeAttackStats
{
    public const float StickySlowFactor = 0.75f;
    public const float StickySizeBonus = 0.08f;
    public const float BaseSplatSize = 1f;
    public const float BasicSplatDuration = 0.3f;
    public const float StickySplatDuration = 5f;
    public const float MultiShotSpreadDegrees = 20f;

    public int ProjectileCount { get; }
    public float SlowMultiplier { get; }
    public float SplatSize { get; }
    public bool HasStickySplat { get; }
    public float SplatDuration { get; }

    public SlimeAttackStats(int projectileCount, float slowMultiplier, float splatSize, bool hasStickySplat, float splatDuration)
    {
        ProjectileCount = Mathf.Max(1, projectileCount);
        SlowMultiplier = Mathf.Clamp(slowMultiplier, 0.05f, 1f);
        SplatSize = Mathf.Max(0.1f, splatSize);
        HasStickySplat = hasStickySplat;
        SplatDuration = splatDuration;
    }

    public static SlimeAttackStats FromSlots(SlimeType[] slots)
    {
        if (slots == null || slots.Length == 0)
            return Default;

        int multiCount = slots.Count(type => type == SlimeType.MultiShot);
        int stickyCount = slots.Count(type => type == SlimeType.Sticky);

        int projectileCount = 1 + multiCount;
        float slowMultiplier = stickyCount > 0
            ? Mathf.Pow(StickySlowFactor, stickyCount)
            : 1f;
        float splatSize = BaseSplatSize + StickySizeBonus * stickyCount;
        bool hasStickySplat = stickyCount > 0;
        float splatDuration = hasStickySplat ? StickySplatDuration : BasicSplatDuration;

        return new SlimeAttackStats(projectileCount, slowMultiplier, splatSize, hasStickySplat, splatDuration);
    }

    public static SlimeAttackStats Default =>
        new SlimeAttackStats(1, 1f, BaseSplatSize, false, BasicSplatDuration);
}
