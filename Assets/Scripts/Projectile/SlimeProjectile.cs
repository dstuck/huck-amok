using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SlimeProjectile : MonoBehaviour
{
    private enum Phase
    {
        Flying,
        Splatting
    }

    [SerializeField] private float splatDuration = 0.3f;

    private Vector2 direction;
    private float speed;
    private float maxRange;
    private float distanceTraveled;
    private Phase phase = Phase.Flying;
    private float splatTimer;
    private bool hasDamagedPlayer;

    public bool IsSplatting => phase == Phase.Splatting;

    public float FlyingProgress =>
        maxRange > 0f ? Mathf.Clamp01(distanceTraveled / maxRange) : 1f;

    public void Initialize(Vector2 fireDirection, float projectileSpeed, float range)
    {
        direction = fireDirection.normalized;
        speed = projectileSpeed;
        maxRange = range;
        distanceTraveled = 0f;
        phase = Phase.Flying;
        splatTimer = 0f;
        hasDamagedPlayer = false;
    }

    private void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.isKinematic = true;
        rb.gravityScale = 0f;

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void Update()
    {
        if (phase == Phase.Flying)
            UpdateFlying();
        else
            UpdateSplat();
    }

    private void UpdateFlying()
    {
        float step = speed * Time.deltaTime;
        transform.Translate(direction * step);
        distanceTraveled += step;

        if (distanceTraveled >= maxRange)
            BeginSplat();
    }

    private void UpdateSplat()
    {
        splatTimer -= Time.deltaTime;
        if (splatTimer <= 0f)
            Destroy(gameObject);
    }

    private void BeginSplat()
    {
        if (phase == Phase.Splatting)
            return;

        phase = Phase.Splatting;
        splatTimer = splatDuration;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (phase == Phase.Splatting)
            return;

        if (other.CompareTag("Player"))
        {
            if (!hasDamagedPlayer && other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(1);
                hasDamagedPlayer = true;
            }

            BeginSplat();
            return;
        }

        if (!other.isTrigger && other.GetComponent<Enemy>() == null)
            BeginSplat();
    }
}
