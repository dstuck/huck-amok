using UnityEngine;

public class EnemyContext
{
    private readonly Enemy enemy;

    public EnemyContext(Enemy enemy, Transform playerTransform)
    {
        this.enemy = enemy;
        Transform = enemy.transform;
        PlayerTransform = playerTransform;
        Tier = enemy.Tier;
        Combine = enemy.GetComponent<IEnemyCombineParticipant>();
    }

    public Transform Transform { get; }
    public Transform PlayerTransform { get; }
    public EnemyTier Tier { get; }
    public IEnemyCombineParticipant Combine { get; }

    public float DistanceToPlayer =>
        PlayerTransform != null
            ? Vector2.Distance(Transform.position, PlayerTransform.position)
            : float.MaxValue;

    public Vector2 DirectionToPlayer =>
        PlayerTransform != null
            ? ((Vector2)PlayerTransform.position - (Vector2)Transform.position).normalized
            : Vector2.zero;

    public void Move(Vector2 worldDirection, float speed)
    {
        enemy.SetMovement(worldDirection, speed, EnemyMovementState.Wandering);
    }

    public void Chase(Vector2 worldDirection, float speed)
    {
        enemy.SetMovement(worldDirection, speed, EnemyMovementState.Chasing);
    }

    public void Stop()
    {
        enemy.StopMovement();
    }

    public SlimeComposition GetComposition()
    {
        return enemy.TryGetComponent<SlimeComposition>(out var composition)
            ? composition
            : null;
    }

    public void SpawnProjectile(
        Vector2 direction,
        EnemyConfig config,
        SlimeAttackStats stats,
        SlimeComposition composition)
    {
        if (config == null || config.projectilePrefab == null)
            return;

        var projectile = Object.Instantiate(
            config.projectilePrefab,
            (Vector2)Transform.position + direction.normalized * 0.06f,
            Quaternion.identity);

        if (projectile.TryGetComponent<SlimeProjectile>(out var slimeProjectile))
            slimeProjectile.Initialize(stats, composition, direction.normalized, config.projectileSpeed, config.projectileMaxRange);
    }

    public void SetFlicker(bool enabled)
    {
        if (enemy.TryGetComponent<InvulnerabilityController>(out var invuln))
            invuln.SetFlickerOverride(enabled);
    }
}
