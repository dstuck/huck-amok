using UnityEngine;

public class SlimeTierLinks : MonoBehaviour
{
    [SerializeField] private GameObject tierUpPrefab;
    [SerializeField] private GameObject tierDownPrefab;
    [SerializeField] private GameObject pickupPiecePrefab;
    [SerializeField] private GameObject pickupRemainderPrefab;
    [SerializeField] private Vector2 pickupRemainderOffset = new Vector2(0.15f, 0f);
    [SerializeField] private bool canAutoCombine;
    [SerializeField] private CombineSettings combineSettings = new CombineSettings();
    [SerializeField] private SlimeMergeRule[] mergeRules;

    public GameObject TierUpPrefab => tierUpPrefab;
    public GameObject TierDownPrefab => tierDownPrefab;
    public GameObject PickupPiecePrefab => pickupPiecePrefab;
    public GameObject PickupRemainderPrefab => pickupRemainderPrefab;
    public Vector2 PickupRemainderOffset => pickupRemainderOffset;
    public bool CanAutoCombine => canAutoCombine;
    public CombineSettings Combine => combineSettings;

    public bool IsAllowedPartnerTier(int partnerTier)
    {
        foreach (var rule in GetEffectiveMergeRules())
        {
            if (rule.partnerTier == partnerTier)
                return true;
        }

        return false;
    }

    public GameObject ResolveMergePrefab(int partnerTier, SlimeTierLinks partnerLinks)
    {
        foreach (var rule in GetEffectiveMergeRules())
        {
            if (rule.partnerTier != partnerTier)
                continue;

            if (rule.usePartnerTierUp)
                return partnerLinks != null ? partnerLinks.TierUpPrefab : null;

            return rule.resultPrefab != null ? rule.resultPrefab : tierUpPrefab;
        }

        return null;
    }

    private SlimeMergeRule[] GetEffectiveMergeRules()
    {
        if (mergeRules != null && mergeRules.Length > 0)
            return mergeRules;

        return DefaultMergeRules;
    }

    private static readonly SlimeMergeRule[] DefaultMergeRules =
    {
        new SlimeMergeRule { partnerTier = 1 },
        new SlimeMergeRule { partnerTier = 2, usePartnerTierUp = true }
    };
}
