using UnityEngine;

/// <summary>
/// Controls player animations using Unity's Animator system.
/// Drives 2D Simple Directional blend trees via DirectionX / DirectionY.
/// </summary>
public class PlayerAnimator : MonoBehaviour
{
    [Header("Animator Reference")]
    [SerializeField] private Animator animator;
    
    private const string PARAM_IS_MOVING = "IsMoving";
    private const string PARAM_IS_CARRYING = "IsCarrying";
    private const string PARAM_DIRECTION_X = "DirectionX";
    private const string PARAM_DIRECTION_Y = "DirectionY";
    
    private Direction currentDirection = Direction.South;
    private bool isCarrying;
    private bool isMoving;
    
    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        
        if (animator == null)
            Debug.LogWarning("PlayerAnimator: No Animator component found.");
    }
    
    private void Start()
    {
        UpdateAnimatorParameters();
    }

    private void Update()
    {
        UpdateAnimatorParameters();
    }
    
    public void OnDirectionChanged(Direction newDirection)
    {
        if (currentDirection == newDirection) return;
        currentDirection = newDirection;
        UpdateAnimatorParameters();
    }
    
    public void OnCarryingStateChanged(bool carrying)
    {
        if (isCarrying == carrying) return;
        isCarrying = carrying;
        UpdateAnimatorParameters();
    }
    
    public void OnMovementStateChanged(bool moving)
    {
        if (isMoving == moving) return;
        isMoving = moving;
        UpdateAnimatorParameters();
    }
    
    private void UpdateAnimatorParameters()
    {
        if (animator == null) return;
        
        animator.SetBool(PARAM_IS_MOVING, isMoving);
        animator.SetBool(PARAM_IS_CARRYING, isCarrying);

        Vector2 facing = DirectionToVector(currentDirection);
        animator.SetFloat(PARAM_DIRECTION_X, facing.x);
        animator.SetFloat(PARAM_DIRECTION_Y, facing.y);
    }
    
    private static Vector2 DirectionToVector(Direction dir)
    {
        switch (dir)
        {
            case Direction.North: return Vector2.up;
            case Direction.South: return Vector2.down;
            case Direction.East: return Vector2.right;
            case Direction.West: return Vector2.left;
            default: return Vector2.down;
        }
    }
}
