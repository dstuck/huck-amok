using UnityEngine;

/// <summary>
/// Controls player animations using Unity's Animator system.
/// Drives 2D Simple Directional blend trees via DirectionX / DirectionY.
/// </summary>
[DefaultExecutionOrder(100)]
public class PlayerAnimator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerController playerController;
    
    private const string PARAM_IS_MOVING = "IsMoving";
    private const string PARAM_IS_CARRYING = "IsCarrying";
    private const string PARAM_DIRECTION_X = "DirectionX";
    private const string PARAM_DIRECTION_Y = "DirectionY";
    
    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (playerController == null)
            playerController = GetComponent<PlayerController>();
        
        if (animator == null)
            Debug.LogWarning("PlayerAnimator: No Animator component found.");
    }

    private void LateUpdate()
    {
        if (animator == null || playerController == null) return;
        
        animator.SetBool(PARAM_IS_MOVING, playerController.IsMoving);
        animator.SetBool(PARAM_IS_CARRYING, playerController.IsCarrying);

        Vector2 facing = DirectionToVector(playerController.CurrentFacing);
        animator.SetFloat(PARAM_DIRECTION_X, facing.x);
        animator.SetFloat(PARAM_DIRECTION_Y, facing.y);
    }
    
    public void OnDirectionChanged(Direction newDirection) { }
    
    public void OnCarryingStateChanged(bool carrying) { }
    
    public void OnMovementStateChanged(bool moving) { }
    
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
