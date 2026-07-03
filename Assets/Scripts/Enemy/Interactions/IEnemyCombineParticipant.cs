using UnityEngine;

/// <summary>
/// Optional capability for enemies that can reserve a partner and merge into a higher tier.
/// </summary>
public interface IEnemyCombineParticipant
{
    bool IsCombining { get; }
    bool CanInitiateCombine { get; }
    CombineTuning CombineTuning { get; }

    bool IsAllowedPartnerTier(int partnerTier);
    Transform TryReservePartner(float seekRadius);
    bool HasReservedPartner { get; }
    Transform ReservedPartnerTransform { get; }
    void ReleasePartner();
    void StartMerge();
}
