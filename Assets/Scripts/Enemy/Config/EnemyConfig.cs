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
