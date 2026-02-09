using UnityEngine;

/// <summary>
/// Controls player animations using Unity's Animator system.
/// This component sets Animator parameters based on player state.
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    [Header("Animator Reference")]
    [SerializeField] private Animator animator;
    
    // Animator parameter names (must match the Animator Controller)
    private const string PARAM_IS_MOVING = "IsMoving";
    private const string PARAM_IS_CARRYING = "IsCarrying";
    private const string PARAM_DIRECTION = "Direction"; // 0=South, 1=North, 2=East, 3=West
    
    private Direction currentDirection = Direction.South;
    private bool isCarrying = false;
    private bool isMoving = false;
    
    private void Awake()
    {
        // Get or find Animator component
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        
        if (animator == null)
        {
            Debug.LogWarning("PlayerAnimator: No Animator component found. Please add an Animator component and assign an Animator Controller.");
        }
    }
    
    private void Start()
    {
        // Initialize animator with current state
        UpdateAnimatorParameters();
    }
    
    /// <summary>
    /// Called when the player's facing direction changes
    /// </summary>
    public void OnDirectionChanged(Direction newDirection)
    {
        if (currentDirection != newDirection)
        {
            currentDirection = newDirection;
            UpdateAnimatorParameters();
        }
    }
    
    /// <summary>
    /// Called when the player's carrying state changes
    /// </summary>
    public void OnCarryingStateChanged(bool carrying)
    {
        if (isCarrying != carrying)
        {
            isCarrying = carrying;
            UpdateAnimatorParameters();
        }
    }
    
    /// <summary>
    /// Called when the player starts or stops moving
    /// </summary>
    public void OnMovementStateChanged(bool moving)
    {
        if (isMoving != moving)
        {
            isMoving = moving;
            UpdateAnimatorParameters();
        }
    }
    
    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;
        
        // Set boolean parameters
        animator.SetBool(PARAM_IS_MOVING, isMoving);
        animator.SetBool(PARAM_IS_CARRYING, isCarrying);
        
        // Set direction as integer (0=South, 1=North, 2=East, 3=West)
        int directionValue = DirectionToInt(currentDirection);
        animator.SetInteger(PARAM_DIRECTION, directionValue);
    }
    
    private int DirectionToInt(Direction dir)
    {
        switch (dir)
        {
            case Direction.South:
                return 2;
            case Direction.North:
                return 0;
            case Direction.East:
                return 1;
            case Direction.West:
                return 3;
            default:
                return 2;
        }
    }
}
