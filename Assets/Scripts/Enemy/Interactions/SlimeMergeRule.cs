using System;
using UnityEngine;

[Serializable]
public struct SlimeMergeRule
{
    [Tooltip("Partner enemy tier (EnemyTier enum value).")]
    public int partnerTier;

    [Tooltip("Prefab spawned when merge completes. When empty, uses this slime's Tier Up prefab.")]
    public GameObject resultPrefab;

    [Tooltip("When enabled, uses the partner's Tier Up prefab instead of Result Prefab.")]
    public bool usePartnerTierUp;
}
