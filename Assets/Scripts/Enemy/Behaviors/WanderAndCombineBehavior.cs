using System;
using UnityEngine;

/// <summary>
/// Composes <see cref="WanderBehavior"/> and intercepts idle expiry to optionally approach a partner slime.
/// Merge execution (reservation, blink, spawn) lives on <see cref="SlimeCombinationController"/>.
/// </summary>
[Serializable]
public class WanderAndCombineBehavior : EnemyBehavior
{
    private enum CombinePhase
    {
        None,
        Approaching,
        Merging
    }

    private readonly WanderBehavior wanderBehavior = new WanderBehavior();
    private CombinePhase combinePhase = CombinePhase.None;

    public override bool TickDuringInvulnerability => true;

    public override void OnEnable(EnemyContext context, EnemyConfig config)
    {
        combinePhase = CombinePhase.None;
        wanderBehavior.SetIdleExpiredHandler(TryBeginApproach);
        wanderBehavior.OnEnable(context, config);
    }

    public override void OnDisable(EnemyContext context)
    {
        context.Combine?.ReleasePartner();
        wanderBehavior.SetIdleExpiredHandler(null);
        wanderBehavior.OnDisable(context);
    }

    public override void Tick(EnemyContext context, EnemyConfig config, float deltaTime)
    {
        var combine = context.Combine;

        if (combine != null && combine.IsCombining)
        {
            combinePhase = CombinePhase.Merging;
            context.Stop();
            return;
        }

        if (combinePhase == CombinePhase.Approaching)
        {
            TickApproaching(context, config, combine);
            return;
        }

        if (combinePhase == CombinePhase.Merging)
        {
            context.Stop();
            return;
        }

        wanderBehavior.Tick(context, config, deltaTime);
    }

    private bool TryBeginApproach(EnemyContext context, EnemyConfig config)
    {
        var combine = context.Combine;
        if (combine == null || !combine.CanInitiateCombine)
            return true;

        var tuning = combine.CombineTuning;
        if (UnityEngine.Random.value >= tuning.DecisionChance)
            return true;

        if (combine.TryReservePartner(tuning.SeekRadius) == null)
            return true;

        combinePhase = CombinePhase.Approaching;
        return false;
    }

    private void TickApproaching(EnemyContext context, EnemyConfig config, IEnemyCombineParticipant combine)
    {
        if (combine == null || !combine.HasReservedPartner)
        {
            combine?.ReleasePartner();
            combinePhase = CombinePhase.None;
            return;
        }

        var partner = combine.ReservedPartnerTransform;
        Vector2 toPartner = (Vector2)partner.position - (Vector2)context.Transform.position;
        float distance = toPartner.magnitude;
        var tuning = combine.CombineTuning;

        if (distance <= tuning.TouchDistance)
        {
            combinePhase = CombinePhase.Merging;
            context.Stop();
            combine.StartMerge();
            return;
        }

        context.Move(toPartner.normalized, config.moveSpeed);
    }
}
