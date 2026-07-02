using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private EnemyTier tier = EnemyTier.Small;

    [Header("Throw")]
    [SerializeField] private float throwTimeout = 0.5f;

    [Header("Movement")]
    [SerializeField] private float directionChangeSmoothing = 3f;

    [Header("Audio")]
    [SerializeField] private AudioClip[] pickupSounds;

    private EnemyState currentState = EnemyState.Active;
    private EnemyMovementState movementState = EnemyMovementState.Idle;
    private Vector2 currentDirection = Vector2.zero;
    private Vector2 targetDirection = Vector2.zero;
    private float currentMoveSpeed;

    private PlayerController heldBy;
    private Vector2 throwDirection;
    private float throwDistance;
    private float throwSpeed;
    private float throwDistanceTraveled;
    private Coroutine throwTimeoutCoroutine;

    private Rigidbody2D rb2d;
    private InvulnerabilityController invulnerability;
    private EnemyBrain brain;

    public EnemyTier Tier => tier;
    public EnemyState CurrentState => currentState;
    public bool IsMoving => movementState != EnemyMovementState.Idle;

    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
            rb2d.isKinematic = true;
            rb2d.gravityScale = 0f;
        }

        invulnerability = GetComponent<InvulnerabilityController>();
        brain = GetComponent<EnemyBrain>();
    }

    private void OnEnable()
    {
        if (GameManager.Instance != null && GameManager.HasStarted)
            GameManager.Instance.RegisterEnemy();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterEnemy();
    }

    private void Update()
    {
        if (currentState == EnemyState.Thrown)
            UpdateThrownMovement();
        else if (currentState == EnemyState.Active)
            UpdateActiveMovement();
    }

    private void UpdateActiveMovement()
    {
        if (movementState == EnemyMovementState.Idle)
            return;

        currentDirection = Vector2.Lerp(
            currentDirection,
            targetDirection,
            directionChangeSmoothing * Time.deltaTime);

        if (currentDirection.sqrMagnitude > 0.0001f)
            currentDirection.Normalize();

        transform.Translate(currentDirection * currentMoveSpeed * Time.deltaTime);
    }

    public void SetMovement(Vector2 worldDirection, float speed, EnemyMovementState state)
    {
        movementState = state;
        targetDirection = worldDirection.sqrMagnitude > 0.0001f ? worldDirection.normalized : Vector2.zero;
        currentMoveSpeed = speed;
        directionChangeSmoothing = brain != null ? brain.DirectionChangeSmoothing : directionChangeSmoothing;
    }

    public void StopMovement()
    {
        movementState = EnemyMovementState.Idle;
        currentDirection = Vector2.zero;
        targetDirection = Vector2.zero;
        currentMoveSpeed = 0f;
    }

    public bool CanBePickedUp()
    {
        if (currentState != EnemyState.Active)
            return false;

        if (invulnerability != null && invulnerability.IsInvulnerable)
            return false;

        return true;
    }

    public void OnPickup(PlayerController player)
    {
        if (!CanBePickedUp())
            return;

        var pickupHandler = GetComponent<IEnemyPickupHandler>();
        if (pickupHandler != null && pickupHandler.OnEnemyPickup(player, this))
            return;

        ApplyPickup(player);
    }

    public void ForcePickup(PlayerController player)
    {
        if (currentState != EnemyState.Active)
            return;

        ApplyPickup(player);
    }

    private void ApplyPickup(PlayerController player)
    {
        currentState = EnemyState.Inactive;
        heldBy = player;
        StopMovement();
        brain?.Pause();

        var col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        if (pickupSounds != null && pickupSounds.Length > 0 && SoundFXManager.Instance != null)
        {
            SoundFXManager.Instance.PlayRandomSoundFXClip(
                pickupSounds,
                transform,
                category: SfxCategory.Pickup);
        }
    }

    public void OnThrown(Vector2 direction, float distance, float speed)
    {
        if (currentState != EnemyState.Inactive)
            return;

        currentState = EnemyState.Thrown;
        throwDirection = direction.normalized;
        throwDistance = distance;
        throwSpeed = speed;
        throwDistanceTraveled = 0f;
        heldBy = null;

        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true;
        }

        if (throwTimeoutCoroutine != null)
            StopCoroutine(throwTimeoutCoroutine);

        throwTimeoutCoroutine = StartCoroutine(ThrowTimeoutCoroutine());
    }

    private void UpdateThrownMovement()
    {
        float moveDistance = throwSpeed * Time.deltaTime;
        transform.Translate(throwDirection * moveDistance);
        throwDistanceTraveled += moveDistance;

        if (throwDistanceTraveled >= throwDistance)
            ReturnToActive();
    }

    private IEnumerator ThrowTimeoutCoroutine()
    {
        yield return new WaitForSeconds(throwTimeout);

        if (currentState == EnemyState.Thrown)
            ReturnToActive();
    }

    private void ReturnToActive()
    {
        if (currentState != EnemyState.Thrown)
            return;

        currentState = EnemyState.Active;
        throwDistanceTraveled = 0f;

        var col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true;
        }

        if (throwTimeoutCoroutine != null)
        {
            StopCoroutine(throwTimeoutCoroutine);
            throwTimeoutCoroutine = null;
        }

        if (invulnerability != null)
            invulnerability.BeginInvulnerability();

        brain?.Resume();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentState != EnemyState.Thrown)
            return;

        var otherEnemy = other.GetComponent<Enemy>();
        if (otherEnemy == null || otherEnemy.currentState != EnemyState.Active)
            return;

        if (otherEnemy.invulnerability != null && otherEnemy.invulnerability.IsInvulnerable)
            return;

        var hitHandler = otherEnemy.GetComponent<IEnemyHitHandler>();
        if (hitHandler != null && hitHandler.OnEnemyHit(this, otherEnemy))
        {
            CancelThrowAndDestroySelf();
            return;
        }

        if (tier == EnemyTier.Small && otherEnemy.tier == EnemyTier.Small)
        {
            CancelThrowAndDestroySelf();
            Destroy(otherEnemy.gameObject);
        }
    }

    private void CancelThrowAndDestroySelf()
    {
        if (throwTimeoutCoroutine != null)
        {
            StopCoroutine(throwTimeoutCoroutine);
            throwTimeoutCoroutine = null;
        }

        Destroy(gameObject);
    }

    public EnemyState GetState() => currentState;
}
