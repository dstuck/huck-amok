using System.Collections;
using UnityEngine;

/// <summary>
/// Handles partner reservation and merge execution. Only tier-1 slimes with
/// <see cref="SlimeTierLinks.CanAutoCombine"/> initiate merges. Higher-tier slimes
/// can be reserved as passive partners (e.g. tier 1 + tier 2 → tier 3).
/// </summary>
[RequireComponent(typeof(Enemy), typeof(SlimeTierLinks))]
public class SlimeCombinationController : MonoBehaviour
{
    private Enemy enemy;
    private EnemyBrain brain;
    private InvulnerabilityController invulnerability;
    private SlimeTierLinks links;
    private SlimeComposition composition;
    private SlimeCombinationController reservedPartner;
    private Coroutine mergeCoroutine;

    public bool IsCombining { get; private set; }

    public bool CanInitiateCombine =>
        links != null && links.CanAutoCombine && (int)enemy.Tier == 1;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        brain = GetComponent<EnemyBrain>();
        invulnerability = GetComponent<InvulnerabilityController>();
        links = GetComponent<SlimeTierLinks>();
        composition = GetComponent<SlimeComposition>();
    }

    public bool IsAvailableAsPartner()
    {
        if (IsCombining || reservedPartner != null)
            return false;

        if (enemy.GetState() != EnemyState.Active)
            return false;

        return invulnerability == null || !invulnerability.IsInvulnerable;
    }

    /// <summary>
    /// Tier-1 initiator only. Finds a tier-1 or tier-2 partner within range and reserves it.
    /// </summary>
    public Transform TryReservePartner(float seekRadius)
    {
        if (!CanInitiateCombine || IsCombining || reservedPartner != null)
            return null;

        if (enemy.GetState() != EnemyState.Active)
            return null;

        if (invulnerability != null && invulnerability.IsInvulnerable)
            return null;

        var hits = Physics2D.OverlapCircleAll(transform.position, seekRadius);
        SlimeCombinationController closest = null;
        float closestDistance = float.MaxValue;

        foreach (var hit in hits)
        {
            if (hit == null || !hit.TryGetComponent<SlimeCombinationController>(out var candidate))
                continue;

            if (candidate == this)
                continue;

            int candidateTier = (int)candidate.enemy.Tier;
            if (candidateTier != 1 && candidateTier != 2)
                continue;

            if (!candidate.IsAvailableAsPartner())
                continue;

            float distance = Vector2.Distance(transform.position, candidate.transform.position);
            if (distance <= seekRadius && distance < closestDistance)
            {
                closest = candidate;
                closestDistance = distance;
            }
        }

        if (closest == null)
            return null;

        ReservePartner(closest);
        return closest.transform;
    }

    private void ReservePartner(SlimeCombinationController partner)
    {
        reservedPartner = partner;
        partner.reservedPartner = this;
        partner.PauseForApproach();
    }

    private void PauseForApproach()
    {
        brain?.Pause();
        enemy.StopMovement();
    }

    public bool HasReservedPartner =>
        reservedPartner != null
        && reservedPartner.reservedPartner == this
        && reservedPartner.enemy.GetState() == EnemyState.Active;

    public Transform ReservedPartnerTransform =>
        reservedPartner != null ? reservedPartner.transform : null;

    public void ReleasePartner()
    {
        var other = reservedPartner;
        reservedPartner = null;

        if (other != null && other.reservedPartner == this)
        {
            other.reservedPartner = null;
            other.ResumeAfterApproach();
        }

        ResumeAfterApproach();
    }

    private void ResumeAfterApproach()
    {
        if (IsCombining)
            return;

        brain?.Resume();
    }

    /// <summary>Called by the tier-1 initiator once the pair is touching.</summary>
    public void StartMerge()
    {
        if (!CanInitiateCombine || IsCombining || !HasReservedPartner)
            return;

        BeginCombiningState();
        reservedPartner.BeginCombiningState();

        mergeCoroutine = StartCoroutine(MergeAfterBlink());
    }

    private void BeginCombiningState()
    {
        IsCombining = true;
        enemy.StopMovement();
    }

    private IEnumerator MergeAfterBlink()
    {
        var other = reservedPartner;

        invulnerability?.SetFlickerOverride(true);
        other.invulnerability?.SetFlickerOverride(true);

        float blinkDuration = ResolveBlinkDuration();
        yield return new WaitForSeconds(blinkDuration);

        if (other == null || other.enemy.GetState() != EnemyState.Active || enemy.GetState() != EnemyState.Active)
        {
            AbortMerge();
            yield break;
        }

        var mergePrefab = ResolveMergePrefab(other);
        if (mergePrefab == null)
        {
            AbortMerge();
            yield break;
        }

        Vector2 midpoint = ((Vector2)transform.position + (Vector2)other.transform.position) * 0.5f;
        var combinedComposition = SlimeComposition.Merge(composition, other.composition);
        SlimeSpawnHelper.Spawn(mergePrefab, midpoint, combinedComposition, beginInvulnerability: true);

        invulnerability?.SetFlickerOverride(false);
        other.invulnerability?.SetFlickerOverride(false);
        IsCombining = false;
        other.IsCombining = false;
        reservedPartner = null;
        other.reservedPartner = null;

        Destroy(other.gameObject);
        Destroy(gameObject);
    }

    private GameObject ResolveMergePrefab(SlimeCombinationController partner)
    {
        int partnerTier = (int)partner.enemy.Tier;

        if (partnerTier == 1)
            return links.TierUpPrefab;

        if (partnerTier == 2)
            return partner.links.TierUpPrefab;

        return null;
    }

    private float ResolveBlinkDuration()
    {
        var config = brain != null ? brain.Config : null;
        return config != null ? config.combineBlinkDuration : 1f;
    }

    private void AbortMerge()
    {
        if (mergeCoroutine != null)
        {
            StopCoroutine(mergeCoroutine);
            mergeCoroutine = null;
        }

        var other = reservedPartner;

        invulnerability?.SetFlickerOverride(false);
        IsCombining = false;
        reservedPartner = null;

        if (other != null)
        {
            other.invulnerability?.SetFlickerOverride(false);
            other.IsCombining = false;
            other.reservedPartner = null;
            other.ResumeAfterApproach();
        }

        ResumeAfterApproach();
    }

    private void OnDisable()
    {
        if (IsCombining)
            AbortMerge();
        else
            ReleasePartner();
    }
}
