using System;
using UnityEngine;

[Serializable]
public class ChaseAndShootBehavior : EnemyBehavior
{
    private enum CombatPhase
    {
        Wander,
        Chase,
        Flicker,
        Cooldown
    }

    private readonly WanderBehavior wanderBehavior = new WanderBehavior();
    private CombatPhase phase = CombatPhase.Wander;
    private float phaseTimer;

    public override void OnEnable(EnemyContext context, EnemyConfig config)
    {
        phase = CombatPhase.Wander;
        wanderBehavior.OnEnable(context, config);
    }

    public override void OnDisable(EnemyContext context)
    {
        context.SetFlicker(false);
        context.Stop();
        wanderBehavior.OnDisable(context);
    }

    public override void Tick(EnemyContext context, EnemyConfig config, float deltaTime)
    {
        float distance = context.DistanceToPlayer;

        if (distance > config.detectionRadius)
        {
            phase = CombatPhase.Wander;
            context.SetFlicker(false);
            wanderBehavior.Tick(context, config, deltaTime);
            return;
        }

        if (distance > config.attackRadius)
        {
            phase = CombatPhase.Chase;
            context.SetFlicker(false);
            context.Chase(context.DirectionToPlayer, config.moveSpeed);
            return;
        }

        if (phase == CombatPhase.Chase || phase == CombatPhase.Wander)
        {
            phase = CombatPhase.Flicker;
            phaseTimer = config.flickerDuration;
            context.SetFlicker(true);
        }

        context.Stop();

        switch (phase)
        {
            case CombatPhase.Flicker:
                phaseTimer -= deltaTime;
                if (phaseTimer <= 0f)
                {
                    context.SetFlicker(false);
                    context.SpawnProjectile(context.DirectionToPlayer, config);
                    phase = CombatPhase.Cooldown;
                    phaseTimer = config.shootCooldown;
                }
                break;

            case CombatPhase.Cooldown:
                phaseTimer -= deltaTime;
                if (phaseTimer <= 0f)
                {
                    phase = CombatPhase.Flicker;
                    phaseTimer = config.flickerDuration;
                    context.SetFlicker(true);
                }
                break;
        }
    }
}
