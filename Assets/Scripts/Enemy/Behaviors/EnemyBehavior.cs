using System;

[Serializable]
public abstract class EnemyBehavior
{
    public virtual bool TickDuringInvulnerability => false;

    public virtual void OnEnable(EnemyContext context, EnemyConfig config) { }

    public abstract void Tick(EnemyContext context, EnemyConfig config, float deltaTime);

    public virtual void OnDisable(EnemyContext context) { }
}
