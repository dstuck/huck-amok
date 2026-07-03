using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyBrain : MonoBehaviour
{
    [SerializeReference] private EnemyBehavior behavior = new WanderBehavior();
    [SerializeField] private EnemyConfig config;

    [Header("Chase And Shoot (double slime)")]
    [SerializeField] private bool useChaseAndShootOverrides;
    [SerializeField] private ChaseAndShootSettings chaseAndShootOverrides = new ChaseAndShootSettings();

    private Enemy enemy;
    private Transform playerTransform;
    private EnemyContext context;
    private bool paused;
    private EnemyConfig runtimeConfig;

    public float DirectionChangeSmoothing =>
        GetEffectiveConfig()?.directionChangeSmoothing ?? 3f;

    public float MoveSpeed =>
        GetEffectiveConfig()?.moveSpeed ?? 0.1f;

    public EnemyConfig Config => GetEffectiveConfig();

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        runtimeConfig = null;
    }

    private EnemyConfig GetEffectiveConfig()
    {
        if (config == null)
            return null;

        if (!useChaseAndShootOverrides || behavior is not ChaseAndShootBehavior)
            return config;

        if (runtimeConfig == null)
            runtimeConfig = Instantiate(config);

        chaseAndShootOverrides.ApplyTo(runtimeConfig);
        return runtimeConfig;
    }

    private void Start()
    {
        var player = FindFirstObjectByType<PlayerController>();
        playerTransform = player != null ? player.transform : null;
        context = new EnemyContext(enemy, playerTransform);
        behavior?.OnEnable(context, GetEffectiveConfig());
    }

    private void OnDestroy()
    {
        behavior?.OnDisable(context);

        if (runtimeConfig != null)
            Destroy(runtimeConfig);
    }

    private void Update()
    {
        if (paused || enemy.GetState() != EnemyState.Active)
            return;

        if (GetComponent<InvulnerabilityController>() is { IsInvulnerable: true } invuln
            && behavior is not WanderBehavior
            && behavior is not WanderAndCombineBehavior)
        {
            return;
        }

        if (playerTransform == null)
        {
            var player = FindFirstObjectByType<PlayerController>();
            if (player != null)
            {
                playerTransform = player.transform;
                context = new EnemyContext(enemy, playerTransform);
            }
        }

        behavior?.Tick(context, GetEffectiveConfig(), Time.deltaTime);
    }

    public void Pause() => paused = true;

    public void Resume()
    {
        paused = false;
        behavior?.OnEnable(context, GetEffectiveConfig());
    }

    public void SetBehavior(EnemyBehavior newBehavior)
    {
        behavior?.OnDisable(context);
        behavior = newBehavior;
        behavior?.OnEnable(context, GetEffectiveConfig());
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (useChaseAndShootOverrides && runtimeConfig != null)
            chaseAndShootOverrides.ApplyTo(runtimeConfig);
    }
#endif
}
