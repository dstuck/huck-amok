# Enemy Framework

Composable enemy architecture for Huck Amok. Enemies share lifecycle and throw/pickup rules; per-type AI and tier interactions are plugged in via composition.

## Script layout

```
Assets/Scripts/Enemy/
├── Core/                    # Lifecycle + orchestration (MonoBehaviours on prefab)
│   ├── Enemy.cs
│   ├── EnemyBrain.cs
│   ├── EnemyContext.cs
│   ├── EnemyState.cs
│   └── EnemyTier.cs
├── Behaviors/               # AI strategies ([Serializable] classes, not MonoBehaviours)
│   ├── EnemyBehavior.cs
│   ├── WanderBehavior.cs
│   ├── WanderAndCombineBehavior.cs
│   └── ChaseAndShootBehavior.cs
├── Config/                  # Data types (C# only; .asset files live elsewhere)
│   ├── EnemyConfig.cs
│   ├── ChaseAndShootSettings.cs
│   └── CombineSettings.cs
├── Interactions/            # Tier-specific pickup / thrown-hit / combine rules
│   ├── IEnemyPickupHandler.cs
│   ├── IEnemyHitHandler.cs
│   ├── IEnemyCombineParticipant.cs
│   ├── SlimeTierLinks.cs
│   ├── SlimeMergeRule.cs
│   ├── SlimeTierInteractions.cs
│   ├── SlimeCombinationController.cs
│   ├── SlimeComposition.cs
│   └── SlimeSpawnHelper.cs
└── Presentation/
    └── EnemyAnimator.cs

Assets/Scripts/Combat/       # Shared combat utilities
├── IDamageable.cs
├── PlayerHealth.cs
└── InvulnerabilityController.cs

Assets/Config/Enemy/         # ScriptableObject tuning assets
├── Tier1SlimeConfig.asset
├── Tier2SlimeConfig.asset
└── Tier3SlimeConfig.asset
```

## Two-layer state model

Lifecycle and combat AI are intentionally separate. Throw physics and pickup do not live inside behavior classes.

```mermaid
stateDiagram-v2
    direction LR
    state lifecycle {
        [*] --> Active
        Active --> Inactive: pickup
        Inactive --> Thrown: throw
        Thrown --> Active: timeout or miss
        Thrown --> [*]: hit enemy
    }
    state combatAI {
        [*] --> Wander
        Wander --> Chase: player in detectionRadius
        Chase --> Attack: player in attackRadius
        Attack --> Chase: player leaves attackRadius
        Chase --> Wander: player leaves detectionRadius
        Attack --> Attack: flicker shoot cooldown
    }
```

| Layer | Where it lives | Examples |
|-------|----------------|----------|
| **Lifecycle** | `Enemy` | Active, Inactive, Thrown |
| **Combat AI** | `EnemyBehavior` subclasses | Wander, Chase, Flicker, Shoot |

`EnemyBrain` only ticks behaviors while `Enemy` is `Active`. Lifecycle pauses AI when Inactive or Thrown. `InvulnerabilityController` can gate pickup and AI during i-frames.

## Composition overview

```mermaid
flowchart TB
    subgraph prefab [Enemy Prefab]
        Enemy
        EnemyBrain
        InvulnerabilityController
        EnemyAnimator
        behaviorRef["SerializeReference EnemyBehavior"]
        links["SlimeTierLinks"]
        handler["SlimeTierInteractions optional"]
        combiner["SlimeCombinationController optional"]
        composition["SlimeComposition"]
    end
    subgraph behaviors [Behaviors]
        WanderBehavior
        WanderAndCombineBehavior
        ChaseAndShootBehavior
    end
    subgraph config [Config]
        EnemyConfigAsset["EnemyConfig .asset"]
        ChaseAndShootSettings
        CombineSettings
    end
    EnemyBrain --> behaviorRef
    behaviorRef --> WanderBehavior
    behaviorRef --> WanderAndCombineBehavior
    behaviorRef --> ChaseAndShootBehavior
    EnemyBrain --> EnemyConfigAsset
    EnemyBrain --> ChaseAndShootSettings
    links --> CombineSettings
    Enemy --> links
    Enemy --> handler
    Enemy --> combiner
    handler --> composition
    combiner --> composition
```

- **Tier-1 slime prefab**: `Enemy` + `WanderAndCombineBehavior` (composes `WanderBehavior`) + `Tier1SlimeConfig` + `SlimeTierLinks` (`canAutoCombine`, `CombineSettings`, merge rules) + `SlimeCombinationController`
- **Tier-2 slime prefab**: `Enemy` + `ChaseAndShootBehavior` (composes `WanderBehavior` when out of range) + `Tier2SlimeConfig` + `SlimeTierLinks` + `SlimeTierInteractions`
- **Tier-3 slime prefab**: `Enemy` + `ChaseAndShootBehavior` + `Tier3SlimeConfig` + `SlimeTierLinks` + `SlimeTierInteractions`
- **Future enemy**: new behavior class + config asset (+ optional interactions handler)

## Behaviors vs Config vs Interactions

```mermaid
flowchart LR
    subgraph scripts [Scripts]
        Behavior["Behaviors/*"]
        ConfigClass["Config/EnemyConfig"]
        Settings["Config/ChaseAndShootSettings"]
    end
    subgraph assets [Assets]
        ConfigAsset["Config/Enemy/*.asset"]
    end
    EnemyBrain --> Behavior
    EnemyBrain --> ConfigAsset
    EnemyBrain --> Settings
    ConfigClass -.defines.-> ConfigAsset
    Enemy --> Handler["Interactions/*"]
```

| Piece | Role | Add when |
|-------|------|----------|
| **Behaviors** | Code — how the enemy thinks and moves | New AI pattern |
| **Config** | Data — speeds, radii, prefab refs | New tunable enemy type |
| **Interactions** | Exceptions to default pickup/throw/combine rules | Split, downgrade, merge |

Behaviors should stay tier-agnostic when possible. Push tier-specific rules and prefab graphs into `Interactions/`. Combine tuning lives on `SlimeTierLinks`, not `EnemyConfig`.

## Core types

| Type | Responsibility |
|------|----------------|
| `Enemy` | Lifecycle, movement application, throw collision routing, `EnemyTier` |
| `EnemyBrain` | Builds `EnemyContext`, calls `EnemyBehavior.Tick()` each frame |
| `EnemyBehavior` | Abstract AI: `OnEnable`, `Tick`, `OnDisable`; optional `TickDuringInvulnerability` |
| `EnemyConfig` | ScriptableObject with shared movement/chase/shoot tuning |
| `EnemyContext` | Per-tick API: distances, `Move`, `Chase`, `Stop`, `SpawnProjectile`, `SetFlicker`, optional `IEnemyCombineParticipant` |
| `ChaseAndShootSettings` | Serializable per-prefab overrides for chase/shoot slime tuning |
| `CombineSettings` | Serializable combine tuning on `SlimeTierLinks` (seek radius, touch distance, blink) |
| `SlimeTierLinks` | Prefab graph for tier transitions, pickup split, merge rules, and combine settings |
| `SlimeTierInteractions` | Generic tier split/downgrade handler driven by `SlimeTierLinks` |
| `SlimeCombinationController` | Partner reservation, merge blink, and spawn (`IEnemyCombineParticipant`) |
| `WanderAndCombineBehavior` | Composes `WanderBehavior`; on idle expiry may approach a partner via `IEnemyCombineParticipant` |
| `SlimeSpawnHelper` | Central spawn path for slimes; future composition data should pass through here |
| `SlimeComposition` | Placeholder for v0.6 type slots, so tier-N slimes can remember which slime types combined |
| `IEnemyPickupHandler` | Optional custom pickup (e.g. tier split) |
| `IEnemyHitHandler` | Optional custom thrown-hit (e.g. tier downgrade) |
| `IEnemyCombineParticipant` | Optional merge capability exposed to behaviors without coupling core to slime types |
| `InvulnerabilityController` | Shared i-frames + flash visual |

## Numeric tiers

`EnemyTier` uses explicit numeric values (`Tier1 = 1`, `Tier2 = 2`, `Tier3 = 3`). Use the enum when tier is identity, and cast to `int` only for tier arithmetic like slot count, one-tier downgrade, or one-tier upgrade checks. Avoid size names in code so new tiers remain numerically extensible.

## Configuring an enemy

### Shared asset

Edit `Assets/Config/Enemy/<Type>Config.asset`. Duplicate for new enemy types.

### Per-prefab overrides (shooting slimes)

On `EnemyBrain`:

1. Assign **Config** asset.
2. Enable **Use Chase And Shoot Overrides** for `ChaseAndShootBehavior` enemies.
3. Tune **Chase And Shoot Overrides** in the Inspector without editing the shared asset.

### Combine tuning (tier-1 slimes)

On `SlimeTierLinks`:

1. Enable **Can Auto Combine** for tier-1 slimes that should merge.
2. Tune **Combine Settings** (seek radius, touch distance, decision chance, blink duration).
3. Configure **Merge Rules** (`partnerTier` → result prefab, or use partner's tier-up prefab).

`WanderAndCombineBehavior` handles seek/approach; `SlimeCombinationController` handles reservation, blink, and spawn.

## Extension checklist

### New AI only (same tier rules)

1. Add `Behaviors/MyBehavior.cs` extending `EnemyBehavior`.
2. Assign via `[SerializeReference]` on `EnemyBrain` (editor script or prefab).
3. Reuse or duplicate an `EnemyConfig` asset.

### New tier with pickup/hit rules

1. Add the next numeric value to `EnemyTier`.
2. Duplicate the previous tier shell prefab and config.
3. Wire `SlimeTierLinks` with `tierDownPrefab`, optional `tierUpPrefab`, `pickupPiecePrefab`, `pickupRemainderPrefab`, and merge rules if this tier can combine.
4. Use `SlimeTierInteractions` for pickup/hit behavior unless the tier needs a nonstandard rule.

### Slime types later

`SlimeTierLinks` should continue to point at tier shell prefabs, not every color/type combination. When v0.6 adds slime types, `SlimeComposition` should carry the ordered type slots and `SlimeSpawnHelper` should apply merged/split composition data after instantiating the tier shell prefab.

### More complex enemies later

- **Composite behaviors** — priority list on `EnemyBrain` (e.g. flee overrides chase).
- **Nested substates** — internal enum inside one `EnemyBehavior` class.
- **Config graphs** — `EnemyConfig` references child config assets for movement vs attack.

## Naming conventions

| Suffix | Use for |
|--------|---------|
| `*Behavior` | AI strategy classes |
| `*Config` | ScriptableObject tuning assets and their C# type |
| `*Settings` | Serializable Inspector override blocks (`ChaseAndShootSettings`, `CombineSettings`) |
| `*Interactions` | Handler components for pickup/throw exceptions |
| `*Links` | Prefab graph data for tier transitions |

## Related docs

- [Project plan](project_plan.md) — version requirements (v0.3 stacked enemies, v0.4 damage)
- [Animation setup](animation_setup.md) — player animator patterns (enemies use `EnemyAnimator` frame cycling instead)
