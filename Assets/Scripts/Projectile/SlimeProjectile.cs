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

    private Rigidbody2D rb2d;
    private BoxCollider2D boxCollider;
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
        direction = fireDirection.sqrMagnitude > 0.0001f ? fireDirection.normalized : Vector2.right;
        speed = projectileSpeed;
        maxRange = range;
        distanceTraveled = 0f;
        phase = Phase.Flying;
        splatTimer = 0f;
        hasDamagedPlayer = false;
    }

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.bodyType = RigidbodyType2D.Kinematic;
        rb2d.gravityScale = 0f;
        rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;

        boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;

        if (boxCollider.size.sqrMagnitude < 0.0001f)
        {
            var sprite = GetComponent<SpriteRenderer>();
            if (sprite != null && sprite.sprite != null)
                boxCollider.size = sprite.sprite.bounds.size;
            else
                boxCollider.size = new Vector2(0.08f, 0.08f);
        }
    }

    private void FixedUpdate()
    {
        if (phase == Phase.Flying)
            UpdateFlying();
    }

    private void Update()
    {
        if (phase == Phase.Splatting)
            UpdateSplat();
    }

    private void UpdateFlying()
    {
        if (speed <= 0f)
            return;

        float step = speed * Time.fixedDeltaTime;
        Vector2 move = direction * step;
        Vector2 start = rb2d.position;

        CheckHitsAlongPath(start, move);

        if (phase != Phase.Flying)
            return;

        rb2d.MovePosition(start + move);
        distanceTraveled += step;

        if (distanceTraveled >= maxRange)
            BeginSplat();
    }

    private void CheckHitsAlongPath(Vector2 start, Vector2 move)
    {
        if (move.sqrMagnitude <= 0f)
            return;

        float distance = move.magnitude;
        Vector2 castDirection = move / distance;
        var hits = Physics2D.BoxCastAll(start, boxCollider.size * 0.9f, 0f, castDirection, distance);

        foreach (var hit in hits)
        {
            if (hit.collider == null || hit.collider == boxCollider)
                continue;

            if (ProcessHit(hit.collider))
                return;
        }
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

        ProcessHit(other);
    }

    private bool ProcessHit(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!hasDamagedPlayer)
            {
                bool isInvulnerable = other.TryGetComponent<InvulnerabilityController>(out var invuln)
                    && invuln.IsInvulnerable;

                if (!isInvulnerable && other.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(1);
                    hasDamagedPlayer = true;
                }
            }

            BeginSplat();
            return true;
        }

        if (!other.isTrigger && other.GetComponent<Enemy>() == null)
        {
            BeginSplat();
            return true;
        }

        return false;
    }
}
