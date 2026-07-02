using UnityEngine;
using UnityEngine.InputSystem;

public enum Direction
{
    North,
    South,
    East,
    West
}

public class PlayerController : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 1f;
    
    [Header("Pickup/Throw")]
    [SerializeField] private float pickupRange = 0.05f;
    [SerializeField] private Vector2 heldOffset = new Vector2(0, 0.15f);
    [SerializeField] private float throwDistance = .75f;
    [SerializeField] private float throwSpeed = 2f;
    [SerializeField] private LayerMask pickupLayerMask = -1;
    
    private InputSystem_Actions inputActions;
    private Vector2 moveInput;
    private Direction currentFacing = Direction.South;
    private Enemy heldEnemy;
    private bool isCarrying = false;
    private Collider2D playerCollider;
    private SpriteRenderer playerSpriteRenderer;
    private PlayerAnimator playerAnimator;
    
    // Animation hooks
    public System.Action<Direction> OnFacingDirectionChanged;
    public System.Action<bool> OnCarryingStateChanged;
    
    public Direction CurrentFacing => currentFacing;
    public bool IsCarrying => isCarrying;
    public bool IsMoving => moveInput.magnitude > 0.1f;

    /// <summary>
    /// Raw facing for blend trees: normalized input while moving, cardinal facing when idle.
    /// </summary>
    public Vector2 AnimationFacing =>
        moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : GetDirectionVector(currentFacing);
    
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.SetCallbacks(this);
        playerCollider = GetComponent<Collider2D>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        playerAnimator = GetComponent<PlayerAnimator>();
    }
    
    private void OnEnable()
    {
        inputActions.Player.Enable();
    }
    
    private void Start()
    {
        // Initialize animator with current state
        if (playerAnimator != null)
        {
            playerAnimator.OnDirectionChanged(currentFacing);
            playerAnimator.OnCarryingStateChanged(isCarrying);
            playerAnimator.OnMovementStateChanged(false);
        }
    }
    
    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
    
    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameplayState.Playing)
            return;

        HandleMovement();
        UpdateHeldObjectPosition();
    }
    
    private void HandleMovement()
    {
        bool isMoving = moveInput.magnitude > 0.1f;

        // Update facing from input even before movement threshold (static direction while idle)
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Direction newFacing = GetDirectionFromVector(moveInput);
            if (newFacing != currentFacing)
            {
                currentFacing = newFacing;
                OnFacingDirectionChanged?.Invoke(currentFacing);
                playerAnimator?.OnDirectionChanged(currentFacing);
            }
        }
        
        if (isMoving)
        {
            // Move the player
            Vector2 movement = moveInput.normalized * moveSpeed * Time.deltaTime;
            transform.Translate(movement);
        }
        
        if (isMoving)
        {
            playerAnimator?.OnMovementStateChanged(true);
        }
        else
        {
            playerAnimator?.OnMovementStateChanged(false);
        }
    }
    
    private Direction GetDirectionFromVector(Vector2 input)
    {
        // Determine primary direction based on input
        float absX = Mathf.Abs(input.x);
        float absY = Mathf.Abs(input.y);
        
        if (absX > absY)
        {
            return input.x > 0 ? Direction.East : Direction.West;
        }
        else
        {
            return input.y > 0 ? Direction.North : Direction.South;
        }
    }
    
    private Vector2 GetDirectionVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.North:
                return Vector2.up;
            case Direction.South:
                return Vector2.down;
            case Direction.East:
                return Vector2.right;
            case Direction.West:
                return Vector2.left;
            default:
                return Vector2.zero;
        }
    }
    
    private Vector2 GetEdgePosition(Direction facing)
    {
        Vector2 center = transform.position;
        Bounds bounds;
        Vector2 facingDir = GetDirectionVector(facing);
        
        // Get bounds from collider or sprite renderer
        if (playerCollider != null)
        {
            bounds = playerCollider.bounds;
        }
        else if (playerSpriteRenderer != null)
        {
            bounds = playerSpriteRenderer.bounds;
        }
        else
        {
            // Fallback: use a small offset from center
            return center + facingDir * 0.1f;
        }
        
        // Get the edge point and offset slightly beyond to avoid hitting own collider
        Vector2 edgePos;
        
        switch (facing)
        {
            case Direction.North:
                edgePos = new Vector2(center.x, bounds.max.y);
                break;
            case Direction.South:
                edgePos = new Vector2(center.x, bounds.min.y);
                break;
            case Direction.East:
                edgePos = new Vector2(bounds.max.x, center.y);
                break;
            case Direction.West:
                edgePos = new Vector2(bounds.min.x, center.y);
                break;
            default:
                edgePos = center;
                break;
        }
        
        // Offset slightly beyond the edge to avoid hitting own collider
        return edgePos + facingDir * 0.01f;
    }
    
    private void UpdateHeldObjectPosition()
    {
        if (heldEnemy != null)
        {
            Vector2 offset = heldOffset;
            heldEnemy.transform.position = (Vector2)transform.position + offset;
        }
    }
    
    private void AttemptPickup()
    {
        if (GameManager.Instance != null && GameManager.Instance.State != GameplayState.Playing)
            return;

        if (heldEnemy != null)
        {
            ThrowHeldObject();
            return;
        }
        
        Vector2 direction = GetDirectionVector(currentFacing);
        
        float overlapRadius = Mathf.Max(pickupRange * 2f, 0.2f);
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, overlapRadius, pickupLayerMask);
        Enemy closestEnemy = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider2D col in overlaps)
        {
            if (col.CompareTag("Player")) continue;
            
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null && enemy.CanBePickedUp())
            {
                Vector2 toEnemy = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;
                float dot = Vector2.Dot(direction, toEnemy);
                
                if (dot > 0.3f)
                {
                    float distance = Vector2.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestEnemy = enemy;
                    }
                }
            }
        }
        
        if (closestEnemy != null)
        {
            PickupEnemy(closestEnemy);
            return;
        }
        
        Vector2 raycastStart = GetEdgePosition(currentFacing);
        
        int playerLayer = gameObject.layer;
        LayerMask excludePlayerMask = pickupLayerMask & ~(1 << playerLayer);
        
        RaycastHit2D hit = Physics2D.Raycast(raycastStart, direction, pickupRange, excludePlayerMask);
        
        if (hit.collider != null)
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null && enemy.CanBePickedUp())
            {
                PickupEnemy(enemy);
            }
        }
    }
    
    private void PickupEnemy(Enemy enemy)
    {
        heldEnemy = enemy;
        isCarrying = true;
        enemy.OnPickup(this);
        UpdateHeldObjectPosition();
        OnCarryingStateChanged?.Invoke(true);
        playerAnimator?.OnCarryingStateChanged(true);
    }

    public void PickupEnemyDirect(Enemy enemy)
    {
        heldEnemy = enemy;
        isCarrying = true;
        enemy.ForcePickup(this);
        UpdateHeldObjectPosition();
        OnCarryingStateChanged?.Invoke(true);
        playerAnimator?.OnCarryingStateChanged(true);
    }
    
    private void ThrowHeldObject()
    {
        if (heldEnemy == null) return;
        
        Vector2 throwDirection = GetDirectionVector(currentFacing);
        Vector2 throwStartPosition = GetThrowStartPosition();
        heldEnemy.transform.position = throwStartPosition;
        
        heldEnemy.OnThrown(throwDirection, throwDistance, throwSpeed);
        heldEnemy = null;
        isCarrying = false;
        OnCarryingStateChanged?.Invoke(false);
        playerAnimator?.OnCarryingStateChanged(false);
    }
    
    private Vector2 GetThrowStartPosition()
    {
        Vector2 playerPos = transform.position;
        Vector2 direction = GetDirectionVector(currentFacing);
        
        // For left/right: place at same Y level, but offset in X direction (in front)
        // For up/down: place overlapping player (same position)
        if (currentFacing == Direction.East || currentFacing == Direction.West)
        {
            // Same Y level, offset in X direction
            return new Vector2(playerPos.x + direction.x * 0.1f, playerPos.y);
        }
        else
        {
            // Up/Down: overlapping player initially
            return playerPos;
        }
    }
    
    // Input System callbacks
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    
    public void OnPickup(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            AttemptPickup();
        }
    }
    
    public void OnSprint(InputAction.CallbackContext context)
    {
        // Not used in v0.1, but required by interface
    }
    
    private void OnDestroy()
    {
        inputActions?.Dispose();
    }
}

