public interface IEnemyHitHandler
{
    /// <summary>
    /// Returns true if the hit was fully handled (no default destroy-both).
    /// </summary>
    bool OnEnemyHit(Enemy thrownEnemy, Enemy targetEnemy);
}
