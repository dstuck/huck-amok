using UnityEngine;

public static class KinematicBody2D
{
    public static Rigidbody2D Configure(GameObject gameObject)
    {
        var rb = gameObject.GetComponent<Rigidbody2D>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        return rb;
    }

    public static void MoveBy(Rigidbody2D rb, Vector2 worldDelta)
    {
        if (rb == null || worldDelta.sqrMagnitude <= 0f)
            return;

        rb.MovePosition(rb.position + worldDelta);
    }

    public static void SetPosition(Rigidbody2D rb, Vector2 worldPosition)
    {
        if (rb == null)
            return;

        rb.position = worldPosition;
    }
}
