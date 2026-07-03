using UnityEngine;

[RequireComponent(typeof(SlimeTierLinks))]
public class SlimeTierInteractions : MonoBehaviour, IEnemyPickupHandler, IEnemyHitHandler
{
    private SlimeTierLinks links;
    private SlimeComposition composition;

    private void Awake()
    {
        links = GetComponent<SlimeTierLinks>();
        composition = GetComponent<SlimeComposition>();
    }

    public bool OnEnemyPickup(PlayerController player, Enemy enemy)
    {
        if ((int)enemy.Tier <= 1)
            return false;

        if (links.PickupPiecePrefab == null || links.PickupRemainderPrefab == null)
        {
            Debug.LogWarning("SlimeTierInteractions: pickup prefab links are not assigned.");
            return false;
        }

        SlimeComposition.SplitForPickup(composition, out var pieceComposition, out var remainderComposition);

        var remainderPosition = (Vector2)enemy.transform.position + links.PickupRemainderOffset;
        SlimeSpawnHelper.Spawn(
            links.PickupRemainderPrefab,
            remainderPosition,
            remainderComposition,
            beginInvulnerability: true);

        var held = SlimeSpawnHelper.Spawn(links.PickupPiecePrefab, enemy.transform.position, pieceComposition);
        var heldEnemy = held != null ? held.GetComponent<Enemy>() : null;

        Destroy(enemy.gameObject);

        if (heldEnemy != null)
            player.PickupEnemyDirect(heldEnemy);

        return true;
    }

    public bool OnEnemyHit(Enemy thrownEnemy, Enemy targetEnemy)
    {
        if ((int)thrownEnemy.Tier != 1 || (int)targetEnemy.Tier <= 1)
            return false;

        if (links.TierDownPrefab == null)
            return false;

        var downgradedComposition = SlimeComposition.SplitForHit(composition);
        SlimeSpawnHelper.Spawn(
            links.TierDownPrefab,
            targetEnemy.transform.position,
            downgradedComposition,
            beginInvulnerability: true);

        Destroy(targetEnemy.gameObject);
        return true;
    }
}
