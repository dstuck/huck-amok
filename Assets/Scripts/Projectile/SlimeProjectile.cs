using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class SlimeProjectile : MonoBehaviour
{
    private enum Phase
    {
        Flying,
        Splatting
    }

    [SerializeField] private AudioClip[] splatSounds;
    [SerializeField] private Sprite stickyPoolSprite;
    [SerializeField] private SlimeTypeDatabase typeDatabase;
    [SerializeField] private Material recolorMaterial;

    private Rigidbody2D rb2d;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    private SlimeVisuals slimeVisuals;
    private Vector2 direction;
    private float speed;
    private float maxRange;
    private float distanceTraveled;
    private Phase phase = Phase.Flying;
    private bool hasDamagedPlayer;
    private SlimeAttackStats attackStats = SlimeAttackStats.Default;

    public bool IsSplatting => phase == Phase.Splatting;

    public float FlyingProgress =>
        maxRange > 0f ? Mathf.Clamp01(distanceTraveled / maxRange) : 1f;

    public void Initialize(
        SlimeAttackStats stats,
        SlimeComposition composition,
        Vector2 fireDirection,
        float projectileSpeed,
        float range)
    {
        attackStats = stats;
        direction = fireDirection.sqrMagnitude > 0.0001f ? fireDirection.normalized : Vector2.right;
        speed = projectileSpeed;
        maxRange = range;
        distanceTraveled = 0f;
        phase = Phase.Flying;
        hasDamagedPlayer = false;

        if (slimeVisuals != null)
            slimeVisuals.ApplyForAttack(attackStats, composition != null ? composition.Slots : null);
    }

    private void Awake()
    {
        rb2d = KinematicBody2D.Configure(gameObject);
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        slimeVisuals = GetComponent<SlimeVisuals>();
        boxCollider.isTrigger = true;

        if (boxCollider.size.sqrMagnitude < 0.0001f)
        {
            if (spriteRenderer != null && spriteRenderer.sprite != null)
                boxCollider.size = spriteRenderer.sprite.bounds.size;
            else
                boxCollider.size = new Vector2(0.08f, 0.08f);
        }
    }

    private void FixedUpdate()
    {
        if (phase == Phase.Flying)
            UpdateFlying();
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

        KinematicBody2D.MoveBy(rb2d, move);
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

    private void BeginSplat()
    {
        if (phase == Phase.Splatting)
            return;

        phase = Phase.Splatting;
        speed = 0f;

        if (!hasDamagedPlayer)
            PlaySplatSound();

        if (attackStats.HasStickySplat)
            SpawnPersistentStickySplat();

        Destroy(gameObject);
    }

    private void SpawnPersistentStickySplat()
    {
        if (stickyPoolSprite == null)
            return;

        var stickyColor = typeDatabase != null
            ? typeDatabase.GetColor(SlimeType.Sticky)
            : new Color(1f, 0.55f, 0.1f);

        var material = recolorMaterial != null
            ? recolorMaterial
            : spriteRenderer != null ? spriteRenderer.sharedMaterial : null;

        StickySplatZone.Spawn(
            transform.position,
            stickyPoolSprite,
            material,
            stickyColor,
            attackStats.SplatSize,
            attackStats.SlowMultiplier,
            attackStats.SplatDuration,
            spriteRenderer != null ? spriteRenderer.sortingLayerID : 0,
            spriteRenderer != null ? spriteRenderer.sortingOrder - 1 : -1);
    }

    private void PlaySplatSound()
    {
        if (splatSounds == null || splatSounds.Length == 0 || SoundFXManager.Instance == null)
            return;

        SoundFXManager.Instance.PlayRandomSoundFXClip(
            splatSounds,
            transform,
            category: SfxCategory.Splat);
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

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (stickyPoolSprite != null)
            return;

        stickyPoolSprite = UnityEditor.AssetDatabase
            .LoadAllAssetsAtPath("Assets/Sprites/pool.png")
            .OfType<Sprite>()
            .FirstOrDefault();
    }
#endif
}
