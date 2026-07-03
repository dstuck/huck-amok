using System;
using UnityEngine;

[Serializable]
public class WanderBehavior : EnemyBehavior
{
    private enum WanderPhase
    {
        Idle,
        Wandering
    }

    private WanderPhase phase = WanderPhase.Idle;
    private float phaseTimer;
    private Vector2 lastWanderDirection = Vector2.right;
    private Func<EnemyContext, EnemyConfig, bool> idleExpiredHandler;

    public override bool TickDuringInvulnerability => true;

    public void SetIdleExpiredHandler(Func<EnemyContext, EnemyConfig, bool> handler)
    {
        idleExpiredHandler = handler;
    }

    public override void OnEnable(EnemyContext context, EnemyConfig config)
    {
        phase = WanderPhase.Idle;
        phaseTimer = UnityEngine.Random.Range(config.idleDurationMin, config.idleDurationMax);
        lastWanderDirection = Vector2.right;
        context.Stop();
    }

    public override void Tick(EnemyContext context, EnemyConfig config, float deltaTime)
    {
        phaseTimer -= deltaTime;

        if (phase == WanderPhase.Idle)
        {
            context.Stop();
            if (phaseTimer <= 0f)
            {
                if (idleExpiredHandler != null && !idleExpiredHandler(context, config))
                {
                    phaseTimer = UnityEngine.Random.Range(config.idleDurationMin, config.idleDurationMax);
                    return;
                }

                EnterWandering(context, config);
            }

            return;
        }

        context.Move(lastWanderDirection, config.moveSpeed);

        if (phaseTimer <= 0f)
            EnterIdle(context, config);
    }

    private void EnterWandering(EnemyContext context, EnemyConfig config)
    {
        phase = WanderPhase.Wandering;
        phaseTimer = UnityEngine.Random.Range(config.wanderDurationMin, config.wanderDurationMax);
        float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
        lastWanderDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        context.Move(lastWanderDirection, config.moveSpeed);
    }

    private void EnterIdle(EnemyContext context, EnemyConfig config)
    {
        phase = WanderPhase.Idle;
        phaseTimer = UnityEngine.Random.Range(config.idleDurationMin, config.idleDurationMax);
        context.Stop();
    }
}
