using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Huck Amok/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [Header("Movement")]
    public float moveSpeed = 0.1f;
    public float directionChangeSmoothing = 3f;

    [Header("Wander")]
    public float wanderDurationMin = 2f;
    public float wanderDurationMax = 4f;
    public float idleDurationMin = 0.5f;
    public float idleDurationMax = 2f;

    [Header("Combine (tier 1 auto-merge)")]
    [Tooltip("How close a partner must be before this slime will consider approaching it to merge. Kept small (~a few slime widths) so merging is a local decision, not a map-wide pull.")]
    public float combineSeekRadius = 0.6f;
    [Tooltip("Distance at which the two slimes are considered touching and start the merge blink. Roughly one slime width.")]
    public float combineTouchDistance = 0.09f;
    [Tooltip("Chance, evaluated once each time the slime leaves idle, that it will approach a nearby partner instead of wandering randomly.")]
    [Range(0f, 1f)]
    public float combineDecisionChance = 0.5f;
    [Tooltip("Seconds the pair blinks before merging into the next tier.")]
    public float combineBlinkDuration = 1f;

    [Header("Chase And Shoot")]
    public float detectionRadius = 4f;
    public float attackRadius = 2f;
    public float minAttackRadius = 0.5f;
    public float flickerDuration = 0.2f;
    public float shootCooldown = 1.5f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public float projectileSpeed = 2f;
    public float projectileMaxRange = 3f;

    [Header("Audio")]
    public AudioClip[] shootSounds;
}
