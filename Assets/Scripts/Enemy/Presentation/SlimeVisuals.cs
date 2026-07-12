using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SlimeVisuals : MonoBehaviour
{
    private const int MaxSlotColors = 3;

    private static readonly int ColorCountId = Shader.PropertyToID("_ColorCount");
    private static readonly int ColorsId = Shader.PropertyToID("_Colors");

    [SerializeField] private SlimeTypeDatabase typeDatabase;
    [SerializeField] private Material recolorMaterial;

    private SpriteRenderer spriteRenderer;
    private MaterialPropertyBlock propertyBlock;
    private readonly Vector4[] slotColors = new Vector4[MaxSlotColors];

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

        int count = Mathf.Min(slots.Length, MaxSlotColors);
        for (int i = 0; i < MaxSlotColors; i++)
        {
            slotColors[i] = i < count
                ? typeDatabase.GetColor(slots[i])
                : Color.white;
        }

        ApplySlotColors(count);
    }

    public void ApplySolidColor(Color color)
    {
        slotColors[0] = color;
        ApplySlotColors(1);
    }

    private void ApplySlotColors(int count)
    {
        spriteRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetFloat(ColorCountId, count);
        propertyBlock.SetVectorArray(ColorsId, slotColors);
        spriteRenderer.SetPropertyBlock(propertyBlock);
    }
}
