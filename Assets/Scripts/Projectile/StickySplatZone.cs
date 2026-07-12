using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
public class StickySplatZone : MonoBehaviour
{
    private static readonly int ColorCountId = Shader.PropertyToID("_ColorCount");
    private static readonly int ColorsId = Shader.PropertyToID("_Colors");

    private readonly HashSet<PlayerMovementSlowdown> overlappingPlayers = new HashSet<PlayerMovementSlowdown>();
    private float slowMultiplier = 1f;

    public float SlowMultiplier => slowMultiplier;

    public static StickySplatZone Spawn(
        Vector2 position,
        Sprite poolSprite,
        Material recolorMaterial,
        Color stickyColor,
        float sizeScale,
        float slowMultiplierValue,
        float duration,
        int sortingLayerId,
        int sortingOrder)
    {
        var splatObject = new GameObject("StickySplat");
        splatObject.transform.position = position;

        var zone = splatObject.AddComponent<StickySplatZone>();
        zone.Configure(
            poolSprite,
            recolorMaterial,
            stickyColor,
            sizeScale,
            slowMultiplierValue,
            duration,
            sortingLayerId,
            sortingOrder);

        return zone;
    }

    private void Configure(
        Sprite poolSprite,
        Material recolorMaterial,
        Color stickyColor,
        float sizeScale,
        float slowMultiplierValue,
        float duration,
        int sortingLayerId,
        int sortingOrder)
    {
        slowMultiplier = slowMultiplierValue;

        var spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = poolSprite;
        spriteRenderer.sharedMaterial = recolorMaterial;
        spriteRenderer.sortingLayerID = sortingLayerId;
        spriteRenderer.sortingOrder = sortingOrder;

        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetFloat(ColorCountId, 1f);
        propertyBlock.SetVectorArray(ColorsId, new[] { (Vector4)stickyColor, (Vector4)Color.white, (Vector4)Color.white });
        spriteRenderer.SetPropertyBlock(propertyBlock);

        transform.localScale = Vector3.one * sizeScale;
        transform.position -= (Vector3)(poolSprite.bounds.center * sizeScale);

        var boxCollider = GetComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
        boxCollider.size = poolSprite.bounds.size;

        Destroy(gameObject, duration);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (other.TryGetComponent<PlayerMovementSlowdown>(out var slowdown))
        {
            overlappingPlayers.Add(slowdown);
            slowdown.EnterZone(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (other.TryGetComponent<PlayerMovementSlowdown>(out var slowdown))
        {
            overlappingPlayers.Remove(slowdown);
            slowdown.ExitZone(this);
        }
    }

    private void OnDisable()
    {
        foreach (var slowdown in overlappingPlayers)
        {
            if (slowdown != null)
                slowdown.ExitZone(this);
        }

        overlappingPlayers.Clear();
    }
}
