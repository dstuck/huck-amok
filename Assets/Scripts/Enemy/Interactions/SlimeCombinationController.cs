using System.Collections;
using UnityEngine;

/// <summary>
/// Handles partner reservation and merge execution. Only slimes with
/// <see cref="SlimeTierLinks.CanAutoCombine"/> at tier 1 initiate merges.
/// Seek/approach movement is driven by <see cref="WanderAndCombineBehavior"/>.
/// </summary>
[RequireComponent(typeof(Enemy), typeof(SlimeTierLinks))]
public class SlimeCombinationController : MonoBehaviour, IEnemyCombineParticipant
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

    public CombineTuning CombineTuning =>
        links != null ? links.Combine.ToTuning() : new CombineTuning(0.6f, 0.09f, 0.5f, 1f);

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        brain = GetComponent<EnemyBrain>();
        invulnerability = GetComponent<InvulnerabilityController>();
        links = GetComponent<SlimeTierLinks>();
        composition = GetComponent<SlimeComposition>();
    }

    public bool IsAllowedPartnerTier(int partnerTier) =>
        links != null && links.IsAllowedPartnerTier(partnerTier);

    public bool IsAvailableAsPartner()
    {
        if (IsCombining || reservedPartner != null)
            return false;

        if (enemy.GetState() != EnemyState.Active)
            return false;

        return invulnerability == null || !invulnerability.IsInvulnerable;
    }

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
            if (!links.IsAllowedPartnerTier(candidateTier))
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

        yield return new WaitForSeconds(CombineTuning.BlinkDuration);

        if (other == null || other.enemy.GetState() != EnemyState.Active || enemy.GetState() != EnemyState.Active)
        {
            AbortMerge();
            yield break;
        }

        var mergePrefab = links.ResolveMergePrefab((int)other.enemy.Tier, other.links);
        if (mergePrefab == null)
        {
            AbortMerge();
            yield break;
        }

        Vector2 midpoint = ((Vector2)transform.position + (Vector2)other.transform.position) * 0.5f;
        var combinedSlots = SlimeComposition.MergeSlots(composition, other.composition);
        SlimeSpawnHelper.Spawn(mergePrefab, midpoint, combinedSlots, beginInvulnerability: true);

        invulnerability?.SetFlickerOverride(false);
        other.invulnerability?.SetFlickerOverride(false);
        IsCombining = false;
        other.IsCombining = false;
        reservedPartner = null;
        other.reservedPartner = null;

        Destroy(other.gameObject);
        Destroy(gameObject);
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
