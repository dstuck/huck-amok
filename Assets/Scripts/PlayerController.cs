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
    private Slime heldSlime;
    private bool isCarrying = false;
    private Collider2D playerCollider;
    private SpriteRenderer playerSpriteRenderer;
    
    // Animation hooks
    public System.Action<Direction> OnFacingDirectionChanged;
    public System.Action<bool> OnCarryingStateChanged;
    
    public Direction CurrentFacing => currentFacing;
    public bool IsCarrying => isCarrying;
    
    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.Player.SetCallbacks(this);
        playerCollider = GetComponent<Collider2D>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void OnEnable()
    {
        inputActions.Player.Enable();
    }
    
    private void OnDisable()
    {
        inputActions.Player.Disable();
    }
    
    private void Update()
    {
        HandleMovement();
        UpdateHeldObjectPosition();
    }
    
    private void HandleMovement()
    {
        if (moveInput.magnitude > 0.1f)
        {
            // Move the player
            Vector2 movement = moveInput.normalized * moveSpeed * Time.deltaTime;
            transform.Translate(movement);
            
            // Update facing direction (snap to N/S/E/W)
            Direction newFacing = GetDirectionFromVector(moveInput);
            if (newFacing != currentFacing)
            {
                currentFacing = newFacing;
                OnFacingDirectionChanged?.Invoke(currentFacing);
            }
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
        if (heldSlime != null)
        {
            Vector2 offset = heldOffset;
            // Adjust offset based on facing direction if needed
            heldSlime.transform.position = (Vector2)transform.position + offset;
        }
    }
    
    private void AttemptPickup()
    {
        if (heldSlime != null)
        {
            // Already holding something, throw it instead
            ThrowHeldObject();
            return;
        }
        
        // Use overlap check first for close-range detection, then raycast for distance
        Vector2 direction = GetDirectionVector(currentFacing);
        
        // First, check for overlapping slimes in the facing direction using overlap
        float overlapRadius = Mathf.Max(pickupRange * 2f, 0.2f); // At least 0.2 units
        Collider2D[] overlaps = Physics2D.OverlapCircleAll(transform.position, overlapRadius, pickupLayerMask);
        Slime closestSlime = null;
        float closestDistance = float.MaxValue;
        
        foreach (Collider2D col in overlaps)
        {
            if (col.CompareTag("Player")) continue;
            
            Slime slime = col.GetComponent<Slime>();
            if (slime != null && slime.CanBePickedUp())
            {
                // Check if slime is in the facing direction
                Vector2 toSlime = ((Vector2)col.transform.position - (Vector2)transform.position).normalized;
                float dot = Vector2.Dot(direction, toSlime);
                
                // If slime is in the general direction (allow some tolerance)
                if (dot > 0.3f) // ~70 degree cone
                {
                    float distance = Vector2.Distance(transform.position, col.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestSlime = slime;
                    }
                }
            }
        }
        
        if (closestSlime != null)
        {
            PickupSlime(closestSlime);
            return;
        }
        
        // Fall back to raycast for objects further away
        // Start raycast from the edge of the player in the facing direction
        Vector2 raycastStart = GetEdgePosition(currentFacing);
        
        // Exclude player layer from the layer mask
        int playerLayer = gameObject.layer;
        LayerMask excludePlayerMask = pickupLayerMask & ~(1 << playerLayer);
        
        RaycastHit2D hit = Physics2D.Raycast(raycastStart, direction, pickupRange, excludePlayerMask);
        
        if (hit.collider != null)
        {
            Slime slime = hit.collider.GetComponent<Slime>();
            if (slime != null && slime.CanBePickedUp())
            {
                PickupSlime(slime);
            }
        }
    }
    
    private void PickupSlime(Slime slime)
    {
        heldSlime = slime;
        isCarrying = true;
        slime.OnPickup(this);
        // Immediately position the slime at the held position
        UpdateHeldObjectPosition();
        OnCarryingStateChanged?.Invoke(true);
    }
    
    private void ThrowHeldObject()
    {
        if (heldSlime == null) return;
        
        Vector2 throwDirection = GetDirectionVector(currentFacing);
        
        // Position the slime correctly before throwing based on facing direction
        Vector2 throwStartPosition = GetThrowStartPosition();
        heldSlime.transform.position = throwStartPosition;
        
        heldSlime.OnThrown(throwDirection, throwDistance, throwSpeed);
        heldSlime = null;
        isCarrying = false;
        OnCarryingStateChanged?.Invoke(false);
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

