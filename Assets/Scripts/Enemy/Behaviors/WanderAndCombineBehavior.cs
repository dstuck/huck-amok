using System;
using UnityEngine;

/// <summary>
/// Wander AI that can also choose to partner up with a nearby slime and merge into the next tier.
/// Merging is a deliberate state transition, not a per-tick probability: each time the slime leaves
/// idle it decides whether to wander randomly or, if a partner happens to be close, approach it.
/// </summary>
[Serializable]
public class WanderAndCombineBehavior : EnemyBehavior
{
    private enum Phase
    {
        Idle,
        Wandering,
        Approaching,
        Merging
    }

    private Phase phase = Phase.Idle;
    private float phaseTimer;
    private Vector2 lastWanderDirection = Vector2.right;

    public override void OnEnable(EnemyContext context, EnemyConfig config)
    {
        phase = Phase.Idle;
        phaseTimer = UnityEngine.Random.Range(config.idleDurationMin, config.idleDurationMax);
        lastWanderDirection = Vector2.right;
        context.Stop();
    }

    public override void OnDisable(EnemyContext context)
    {
        context.Combination?.ReleasePartner();
        context.Stop();
    }

    public override void Tick(EnemyContext context, EnemyConfig config, float deltaTime)
    {
        // Once the controller has committed this slime to a merge (as leader or follower),
        // freeze into the merging state until the blink + spawn completes.
        if (context.Combination != null && context.Combination.IsCombining)
        {
            phase = Phase.Merging;
            context.Stop();
            return;
        }

        switch (phase)
        {
            case Phase.Idle:
                TickIdle(context, config, deltaTime);
                break;
            case Phase.Wandering:
                TickWandering(context, config, deltaTime);
                break;
            case Phase.Approaching:
                TickApproaching(context, config);
                break;
            case Phase.Merging:
                // Combination controller owns the blink + spawn; nothing to do here.
                context.Stop();
                break;
        }
    }

    private void TickIdle(EnemyContext context, EnemyConfig config, float deltaTime)
    {
        context.Stop();
        phaseTimer -= deltaTime;
        if (phaseTimer > 0f)
            return;

        if (TryDecideToPartner(context, config))
            return;

        EnterWandering(context, config);
    }

    private bool TryDecideToPartner(EnemyContext context, EnemyConfig config)
    {
        var combination = context.Combination;
        if (combination == null || !combination.CanInitiateCombine)
            return false;

        if (UnityEngine.Random.value >= config.combineDecisionChance)
            return false;

        if (combination.TryReservePartner(config.combineSeekRadius) == null)
            return false;

        phase = Phase.Approaching;
        return true;
    }

    private void EnterWandering(EnemyContext context, EnemyConfig config)
    {
        phase = Phase.Wandering;
        phaseTimer = UnityEngine.Random.Range(config.wanderDurationMin, config.wanderDurationMax);
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        lastWanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        context.Move(lastWanderDirection, config.moveSpeed);
    }

    private void TickWandering(EnemyContext context, EnemyConfig config, float deltaTime)
    {
        context.Move(lastWanderDirection, config.moveSpeed);

        phaseTimer -= deltaTime;
        if (phaseTimer <= 0f)
            EnterIdle(context, config);
    }

    private void EnterIdle(EnemyContext context, EnemyConfig config)
    {
        phase = Phase.Idle;
        phaseTimer = UnityEngine.Random.Range(config.idleDurationMin, config.idleDurationMax);
        context.Stop();
    }

    private void TickApproaching(EnemyContext context, EnemyConfig config)
    {
        var combination = context.Combination;

        if (combination == null || !combination.HasReservedPartner)
        {
            combination?.ReleasePartner();
            EnterIdle(context, config);
            return;
        }

        var partner = combination.ReservedPartnerTransform;
        Vector2 toPartner = (Vector2)partner.position - (Vector2)context.Transform.position;
        float distance = toPartner.magnitude;

        if (distance <= config.combineTouchDistance)
        {
            phase = Phase.Merging;
            context.Stop();
            combination.StartMerge();
            return;
        }

        context.Move(toPartner.normalized, config.moveSpeed);
    }
}
