using UnityEngine;
using System.Collections;

public enum SlimeState
{
    Active,
    Inactive,
    Thrown
}

public enum SlimeMovementState
{
    Idle,
    Wandering,
    Chasing // For future use
}

public class Slime : MonoBehaviour
{
    [Header("Throw Settings")]
    [SerializeField] private float throwTimeout = 0.5f;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 0.1f;
    [SerializeField] private float wanderDurationMin = 2f;
    [SerializeField] private float wanderDurationMax = 4f;
    [SerializeField] private float idleDurationMin = 0.5f;
    [SerializeField] private float idleDurationMax = 2f;
    [SerializeField] private float directionChangeSmoothing = 3f; // How quickly to change direction
    
    private SlimeState currentState = SlimeState.Active;
    private SlimeMovementState movementState = SlimeMovementState.Idle;
    private PlayerController heldBy;
    private Vector2 throwDirection;
    private float throwDistance;
    private float throwSpeed;
    private Vector2 throwStartPosition;
    private float throwDistanceTraveled = 0f;
    private Coroutine throwTimeoutCoroutine;
    private Coroutine movementCoroutine;
    private Rigidbody2D rb2d;
    
    // Movement variables
    private Vector2 currentDirection = Vector2.zero;
    private Vector2 targetDirection = Vector2.zero;
    
    private void Awake()
    {
        // Ensure we have a Rigidbody2D for trigger detection
        rb2d = GetComponent<Rigidbody2D>();
        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
            rb2d.isKinematic = true; // Don't let physics move it, we control movement manually
            rb2d.gravityScale = 0; // No gravity
            Debug.Log($"[Slime {gameObject.name}] Added Rigidbody2D for trigger detection");
        }
    }
    
    private void Start()
    {
        // Start movement when slime becomes active
        if (currentState == SlimeState.Active)
        {
            StartMovement();
        }
    }
    
    private void Update()
    {
        if (currentState == SlimeState.Thrown)
        {
            UpdateThrownMovement();
        }
        else if (currentState == SlimeState.Active)
        {
            UpdateActiveMovement();
        }
    }
    
    private void UpdateActiveMovement()
    {
        // Smoothly transition to target direction
        if (movementState == SlimeMovementState.Wandering)
        {
            currentDirection = Vector2.Lerp(currentDirection, targetDirection, directionChangeSmoothing * Time.deltaTime);
            currentDirection.Normalize();
            
            // Move in current direction
            transform.Translate(currentDirection * moveSpeed * Time.deltaTime);
        }
    }
    
    public bool CanBePickedUp()
    {
        return currentState == SlimeState.Active;
    }

    public bool IsMoving => movementState == SlimeMovementState.Wandering;
    
    public void OnPickup(PlayerController player)
    {
        if (currentState != SlimeState.Active) return;
        
        currentState = SlimeState.Inactive;
        heldBy = player;
        
        // Stop movement
        StopMovement();
        
        // Disable collision/physics while held
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
            Debug.Log($"[Slime {gameObject.name}] OnPickup - Collider disabled");
        }
    }
    
    public void OnThrown(Vector2 direction, float distance, float speed)
    {
        if (currentState != SlimeState.Inactive) return;
        
        currentState = SlimeState.Thrown;
        throwDirection = direction.normalized;
        throwDistance = distance;
        throwSpeed = speed;
        throwStartPosition = transform.position;
        throwDistanceTraveled = 0f;
        
        // Re-enable collision for thrown state (keep as trigger)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true; // Always use trigger so it doesn't block movement
            Debug.Log($"[Slime {gameObject.name}] OnThrown - Collider enabled: {col.enabled}, isTrigger: {col.isTrigger}, layer: {gameObject.layer}");
        }
        else
        {
            Debug.LogWarning($"[Slime {gameObject.name}] OnThrown - No collider found!");
        }
        
        // Start timeout coroutine
        if (throwTimeoutCoroutine != null)
        {
            StopCoroutine(throwTimeoutCoroutine);
        }
        throwTimeoutCoroutine = StartCoroutine(ThrowTimeoutCoroutine());
        
        heldBy = null;
    }
    
    private void UpdateThrownMovement()
    {
        float moveDistance = throwSpeed * Time.deltaTime;
        Vector2 movement = throwDirection * moveDistance;
        transform.Translate(movement);
        
        throwDistanceTraveled += moveDistance;
        
        // Check if we've traveled the throw distance
        if (throwDistanceTraveled >= throwDistance)
        {
            ReturnToActive();
        }
    }
    
    private IEnumerator ThrowTimeoutCoroutine()
    {
        yield return new WaitForSeconds(throwTimeout);
        
        // If still in thrown state and haven't hit anything, return to active
        if (currentState == SlimeState.Thrown)
        {
            ReturnToActive();
        }
    }
    
    private void ReturnToActive()
    {
        if (currentState != SlimeState.Thrown) return;
        
        currentState = SlimeState.Active;
        throwDistanceTraveled = 0f;
        
        // Keep collider enabled and as trigger (so it doesn't block movement)
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
            col.isTrigger = true; // Stay as trigger so it doesn't block movement
        }
        
        if (throwTimeoutCoroutine != null)
        {
            StopCoroutine(throwTimeoutCoroutine);
            throwTimeoutCoroutine = null;
        }
        
        // Start movement when returning to active
        StartMovement();
    }
    
    private void StartMovement()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
        }
        movementCoroutine = StartCoroutine(MovementCoroutine());
    }
    
    private void StopMovement()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
        movementState = SlimeMovementState.Idle;
        currentDirection = Vector2.zero;
        targetDirection = Vector2.zero;
    }
    
    private IEnumerator MovementCoroutine()
    {
        while (currentState == SlimeState.Active)
        {
            // Idle for a random duration
            movementState = SlimeMovementState.Idle;
            currentDirection = Vector2.zero;
            targetDirection = Vector2.zero;
            yield return new WaitForSeconds(Random.Range(idleDurationMin, idleDurationMax));
            
            if (currentState != SlimeState.Active) break;
            
            // Wander in a random direction
            movementState = SlimeMovementState.Wandering;
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            targetDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            
            // Wander for a random duration
            float wanderTime = Random.Range(wanderDurationMin, wanderDurationMax);
            yield return new WaitForSeconds(wanderTime);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[Slime {gameObject.name}] OnTriggerEnter2D called - Current state: {currentState}, Other: {other.name}, Other tag: {other.tag}");
        
        if (currentState != SlimeState.Thrown)
        {
            Debug.Log($"[Slime {gameObject.name}] Not in Thrown state, ignoring collision");
            return;
        }
        
        // Check if we hit another slime
        Slime otherSlime = other.GetComponent<Slime>();
        if (otherSlime != null)
        {
            Debug.Log($"[Slime {gameObject.name}] Hit slime {otherSlime.name}, other state: {otherSlime.currentState}");
            if (otherSlime.currentState == SlimeState.Active)
            {
                Debug.Log($"[Slime {gameObject.name}] Destroying both slimes!");
                // Stop timeout coroutine before destroying
                if (throwTimeoutCoroutine != null)
                {
                    StopCoroutine(throwTimeoutCoroutine);
                    throwTimeoutCoroutine = null;
                }
                
                // Destroy both slimes
                Destroy(otherSlime.gameObject);
                Destroy(gameObject);
            }
            else
            {
                Debug.Log($"[Slime {gameObject.name}] Other slime is not Active (state: {otherSlime.currentState}), not destroying");
            }
        }
        else
        {
            Debug.Log($"[Slime {gameObject.name}] Hit object {other.name} but no Slime component found");
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[Slime {gameObject.name}] OnCollisionEnter2D called - Current state: {currentState}, Other: {collision.gameObject.name}");
    }
    
    public SlimeState GetState()
    {
        return currentState;
    }
}

