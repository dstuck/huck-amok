using System;
using UnityEngine;

/// <summary>
/// Per-enemy chase/shoot tuning. Assign on EnemyBrain to override values from the config asset.
/// </summary>
[Serializable]
public class ChaseAndShootSettings
{
    [Header("Movement")]
    [Tooltip("Chase speed when pursuing the player.")]
    public float moveSpeed = 0.2f;

    [Header("Ranges")]
    [Tooltip("Player must be within this distance before the enemy engages.")]
    public float detectionRadius = 4f;

    [Tooltip("Enemy stops and shoots when the player is within this distance.")]
    public float attackRadius = 1f;

    [Tooltip("Enemy backs away if closer than this while attacking.")]
    public float minAttackRadius = 0.5f;

    [Header("Attack Timing")]
    public float flickerDuration = 0.2f;
    public float shootCooldown = 1.5f;

    [Header("Projectile")]
    public float projectileSpeed = 1.4f;
    public float projectileMaxRange = 3f;

    public void ApplyTo(EnemyConfig config)
    {
        if (config == null)
            return;

        config.moveSpeed = moveSpeed;
        config.detectionRadius = detectionRadius;
        config.attackRadius = attackRadius;
        config.minAttackRadius = minAttackRadius;
        config.flickerDuration = flickerDuration;
        config.shootCooldown = shootCooldown;
        config.projectileSpeed = projectileSpeed;
        config.projectileMaxRange = projectileMaxRange;
    }
}
