using System.Linq;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WeightedSpriteAnimator : MonoBehaviour
{
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float[] flyingFrameWeights = { 0.50f, 0.25f, 0.15f };

    private SpriteRenderer spriteRenderer;
    private SlimeProjectile projectile;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        projectile = GetComponent<SlimeProjectile>();
    }

    private void Update()
    {
        if (frames == null || frames.Length == 0)
            return;

        if (projectile != null && projectile.IsSplatting)
        {
            spriteRenderer.sprite = frames[frames.Length - 1];
            return;
        }

        float normalized = projectile != null ? projectile.FlyingProgress : 0f;
        spriteRenderer.sprite = frames[GetFlyingFrameIndex(normalized)];
    }

    private int GetFlyingFrameIndex(float normalizedTime)
    {
        int flyingFrameCount = Mathf.Min(frames.Length - 1, flyingFrameWeights.Length);
        if (flyingFrameCount <= 0)
            return 0;

        float weightSum = 0f;
        for (int i = 0; i < flyingFrameCount; i++)
            weightSum += flyingFrameWeights[i];

        float cumulative = 0f;
        for (int i = 0; i < flyingFrameCount; i++)
        {
            cumulative += flyingFrameWeights[i] / weightSum;
            if (normalizedTime < cumulative)
                return i;
        }

        return flyingFrameCount - 1;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (frames != null && frames.Length > 0)
            return;

        frames = UnityEditor.AssetDatabase
            .LoadAllAssetsAtPath("Assets/Sprites/slimeProjectile.png")
            .OfType<Sprite>()
            .OrderBy(sprite => sprite.name)
            .ToArray();
    }
#endif
}
