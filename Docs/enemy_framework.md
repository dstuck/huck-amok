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
│   └── ChaseAndShootBehavior.cs
├── Config/                  # Data types (C# only; .asset files live elsewhere)
│   ├── EnemyConfig.cs
│   └── ChaseAndShootSettings.cs
├── Interactions/            # Tier-specific pickup / thrown-hit rules
│   ├── IEnemyPickupHandler.cs
│   ├── IEnemyHitHandler.cs
│   └── DoubleSlimeInteractions.cs
└── Presentation/
    └── EnemyAnimator.cs

Assets/Scripts/Combat/       # Shared combat utilities
├── IDamageable.cs
├── PlayerHealth.cs
└── InvulnerabilityController.cs

Assets/Config/Enemy/         # ScriptableObject tuning assets
├── SmallSlimeConfig.asset
└── DoubleSlimeConfig.asset
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
        handler["Interactions handler optional"]
    end
    subgraph behaviors [Behaviors]
        WanderBehavior
        ChaseAndShootBehavior
    end
    subgraph config [Config]
        EnemyConfigAsset["EnemyConfig .asset"]
        ChaseAndShootSettings
    end
    EnemyBrain --> behaviorRef
    behaviorRef --> WanderBehavior
    behaviorRef --> ChaseAndShootBehavior
    EnemyBrain --> EnemyConfigAsset
    EnemyBrain --> ChaseAndShootSettings
    Enemy --> handler
```

- **Small slime prefab**: `Enemy` + `WanderBehavior` + `SmallSlimeConfig`
- **Double slime prefab**: `Enemy` + `ChaseAndShootBehavior` + `DoubleSlimeConfig` + `DoubleSlimeInteractions`
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
| **Interactions** | Exceptions to default pickup/throw rules | Split, downgrade, special hit logic |

Behaviors should stay tier-agnostic when possible. Push tier-specific rules into `Interactions/`.

## Core types

| Type | Responsibility |
|------|----------------|
| `Enemy` | Lifecycle, movement application, throw collision routing, `EnemyTier` |
| `EnemyBrain` | Builds `EnemyContext`, calls `EnemyBehavior.Tick()` each frame |
| `EnemyBehavior` | Abstract AI: `OnEnable`, `Tick`, `OnDisable` |
| `EnemyConfig` | ScriptableObject with shared tuning fields |
| `EnemyContext` | Per-tick API: distances, `Move`, `Chase`, `Stop`, `SpawnProjectile`, `SetFlicker` |
| `ChaseAndShootSettings` | Serializable per-prefab overrides for double slime tuning |
| `IEnemyPickupHandler` | Optional custom pickup (e.g. medium split) |
| `IEnemyHitHandler` | Optional custom thrown-hit (e.g. medium → small downgrade) |
| `InvulnerabilityController` | Shared i-frames + flash visual |

## Configuring an enemy

### Shared asset

Edit `Assets/Config/Enemy/<Type>Config.asset`. Duplicate for new enemy types.

### Per-prefab overrides (double slime)

On `EnemyBrain`:

1. Assign **Config** asset.
2. Enable **Use Chase And Shoot Overrides** for `ChaseAndShootBehavior` enemies.
3. Tune **Chase And Shoot Overrides** in the Inspector without editing the shared asset.

## Extension checklist

### New AI only (same tier rules)

1. Add `Behaviors/MyBehavior.cs` extending `EnemyBehavior`.
2. Assign via `[SerializeReference]` on `EnemyBrain` (editor script or prefab).
3. Reuse or duplicate an `EnemyConfig` asset.

### New enemy type with special pickup/hit rules

1. Above steps, plus `Interactions/MyEnemyInteractions.cs` implementing handler interfaces.
2. Add component to prefab.
3. Wire any prefab refs on the handler (e.g. remainder spawn prefab).

### More complex enemies later

- **Composite behaviors** — priority list on `EnemyBrain` (e.g. flee overrides chase).
- **Nested substates** — internal enum inside one `EnemyBehavior` class.
- **Config graphs** — `EnemyConfig` references child config assets for movement vs attack.

## Naming conventions

| Suffix | Use for |
|--------|---------|
| `*Behavior` | AI strategy classes |
| `*Config` | ScriptableObject tuning assets and their C# type |
| `*Settings` | Serializable Inspector override blocks |
| `*Interactions` | Handler components for pickup/throw exceptions |

## Related docs

- [Project plan](project_plan.md) — version requirements (v0.3 stacked enemies, v0.4 damage)
- [Animation setup](animation_setup.md) — player animator patterns (enemies use `EnemyAnimator` frame cycling instead)
