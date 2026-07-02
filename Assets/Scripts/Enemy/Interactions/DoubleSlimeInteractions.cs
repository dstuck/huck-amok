using UnityEngine;

public class DoubleSlimeInteractions : MonoBehaviour, IEnemyPickupHandler, IEnemyHitHandler
{
    [SerializeField] private GameObject smallSlimePrefab;
    [SerializeField] private Vector2 remainderOffset = new Vector2(0.15f, 0f);

    public bool OnEnemyPickup(PlayerController player, Enemy enemy)
    {
        if (enemy.Tier != EnemyTier.Medium)
            return false;

        if (smallSlimePrefab == null)
        {
            Debug.LogWarning("DoubleSlimeInteractions: smallSlimePrefab not assigned.");
            return false;
        }

        var remainderPosition = (Vector2)enemy.transform.position + remainderOffset;
        var remainder = Instantiate(smallSlimePrefab, remainderPosition, Quaternion.identity);
        if (remainder.TryGetComponent<InvulnerabilityController>(out var remainderInvuln))
            remainderInvuln.BeginInvulnerability();

        var held = Instantiate(smallSlimePrefab, enemy.transform.position, Quaternion.identity);
        var heldEnemy = held.GetComponent<Enemy>();
        Destroy(enemy.gameObject);
        if (heldEnemy != null)
            player.PickupEnemyDirect(heldEnemy);
        return true;
    }

    public bool OnEnemyHit(Enemy thrownEnemy, Enemy targetEnemy)
    {
        if (targetEnemy.Tier != EnemyTier.Medium || thrownEnemy.Tier != EnemyTier.Small)
            return false;

        if (smallSlimePrefab == null)
            return false;

        var position = targetEnemy.transform.position;
        var small = Instantiate(smallSlimePrefab, position, Quaternion.identity);

        if (small.TryGetComponent<InvulnerabilityController>(out var invuln))
            invuln.BeginInvulnerability();

        Destroy(targetEnemy.gameObject);
        return true;
    }
}
