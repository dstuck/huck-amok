using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Enemy))]
public class EnemyAnimator : MonoBehaviour
{
    [Header("Sprites")]
    [SerializeField] private Sprite[] idleFrames;

    [Header("Timing")]
    [SerializeField] private float frameDuration = 0.15f;
    [SerializeField] private float speedVariation = 0.25f;
    [SerializeField] private float idleSpeedMultiplier = 0f;

    private SpriteRenderer spriteRenderer;
    private Enemy enemy;
    private float frameTimer;
    private int frameIndex;
    private float animationSpeed = 1f;
    private float localFrameDuration;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        enemy = GetComponent<Enemy>();

        animationSpeed = 1f + Random.Range(-speedVariation, speedVariation);
        localFrameDuration = frameDuration * (1f + Random.Range(-0.15f, 0.15f));

        if (idleFrames != null && idleFrames.Length > 0)
        {
            frameIndex = Random.Range(0, idleFrames.Length);
            frameTimer = Random.Range(0f, localFrameDuration);
        }
    }

    private void Update()
    {
        if (idleFrames == null || idleFrames.Length == 0)
            return;

        float speed = GetPlaybackSpeed();
        if (speed <= 0f)
        {
            spriteRenderer.sprite = idleFrames[frameIndex];
            return;
        }

        frameTimer += Time.deltaTime * speed;
        while (frameTimer >= localFrameDuration)
        {
            frameTimer -= localFrameDuration;
            frameIndex = (frameIndex + 1) % idleFrames.Length;
        }

        spriteRenderer.sprite = idleFrames[frameIndex];
    }

    private float GetPlaybackSpeed()
    {
        if (enemy == null)
            return animationSpeed;

        switch (enemy.GetState())
        {
            case EnemyState.Inactive:
                return 0f;
            case EnemyState.Thrown:
                return animationSpeed;
            case EnemyState.Active:
                return enemy.IsMoving ? animationSpeed : animationSpeed * idleSpeedMultiplier;
            default:
                return animationSpeed;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (idleFrames != null && idleFrames.Length > 0)
            return;

        string path = GetComponent<Enemy>()?.Tier == EnemyTier.Medium
            ? "Assets/Sprites/doubleSlime.png"
            : "Assets/Sprites/sprSlimeIdle.png";

        idleFrames = UnityEditor.AssetDatabase
            .LoadAllAssetsAtPath(path)
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name)
            .ToArray();
    }
#endif
}
