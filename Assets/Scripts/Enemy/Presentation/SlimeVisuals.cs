using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SlimeVisuals : MonoBehaviour
{
    private static readonly int ColorAId = Shader.PropertyToID("_ColorA");
    private static readonly int ColorBId = Shader.PropertyToID("_ColorB");
    private static readonly int UseGradientId = Shader.PropertyToID("_UseGradient");
    private static readonly int GradientAxisId = Shader.PropertyToID("_GradientAxis");

    [SerializeField] private SlimeTypeDatabase typeDatabase;
    [SerializeField] private Material recolorMaterial;

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        propertyBlock = new MaterialPropertyBlock();

        if (recolorMaterial != null)
            spriteRenderer.sharedMaterial = recolorMaterial;
    }

    public void Apply(SlimeComposition composition)
    {
        if (composition == null || typeDatabase == null || spriteRenderer == null)
            return;

        ApplySlots(composition.Slots);
    }

    public void ApplyForAttack(SlimeAttackStats stats, SlimeType[] slots)
    {
        if (stats.HasStickySplat)
        {
            var visualSlots = stats.ProjectileCount > 1
                ? new[] { SlimeType.Sticky, SlimeType.MultiShot }
                : new[] { SlimeType.Sticky };
            ApplySlots(visualSlots);
            return;
        }

        ApplySlots(slots);
    }

    public void ApplySlots(SlimeType[] slots)
    {
        if (typeDatabase == null || spriteRenderer == null)
            return;

        if (slots == null || slots.Length == 0)
            slots = new[] { SlimeType.Basic };

        Color colorA = typeDatabase.GetColor(slots[0]);
        Color colorB = slots.Length > 1
            ? typeDatabase.GetColor(slots[slots.Length - 1])
            : colorA;

        ApplyColors(colorA, colorB, slots.Length > 1);
    }

    public void ApplySolidColor(Color color)
    {
        ApplyColors(color, color, useGradient: false);
    }

    private void ApplyColors(Color colorA, Color colorB, bool useGradient)
    {
        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(ColorAId, colorA);
        propertyBlock.SetColor(ColorBId, colorB);
        propertyBlock.SetFloat(UseGradientId, useGradient ? 1f : 0f);
        propertyBlock.SetVector(GradientAxisId, new Vector4(0f, 1f, 0f, 0f));
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }
}
