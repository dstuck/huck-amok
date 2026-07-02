public interface IEnemyPickupHandler
{
    /// <summary>
    /// Returns true if pickup was fully handled (skip default pickup logic).
    /// </summary>
    bool OnEnemyPickup(PlayerController player, Enemy enemy);
}
