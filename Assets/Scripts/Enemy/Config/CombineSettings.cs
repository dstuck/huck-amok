using System;
using UnityEngine;

/// <summary>
/// Per-slime combine tuning. Lives on <see cref="SlimeTierLinks"/> for tier-1 auto-merge slimes.
/// </summary>
[Serializable]
public class CombineSettings
{
    [Tooltip("How close a partner must be before this slime will consider approaching it to merge.")]
    public float seekRadius = 0.6f;

    [Tooltip("Distance at which the two slimes are considered touching and start the merge blink.")]
    public float touchDistance = 0.09f;

    [Tooltip("Chance, evaluated once each time the slime leaves idle, that it will approach a nearby partner.")]
    [Range(0f, 1f)]
    public float decisionChance = 0.5f;

    [Tooltip("Seconds the pair blinks before merging into the next tier.")]
    public float blinkDuration = 1f;

    public CombineTuning ToTuning() =>
        new CombineTuning(seekRadius, touchDistance, decisionChance, blinkDuration);
}

public readonly struct CombineTuning
{
    public float SeekRadius { get; }
    public float TouchDistance { get; }
    public float DecisionChance { get; }
    public float BlinkDuration { get; }

    public CombineTuning(float seekRadius, float touchDistance, float decisionChance, float blinkDuration)
    {
        SeekRadius = seekRadius;
        TouchDistance = touchDistance;
        DecisionChance = decisionChance;
        BlinkDuration = blinkDuration;
    }
}
