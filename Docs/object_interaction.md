# Object Interaction & Manipulation

Reference for how gameplay objects move, collide, and delegate behavior.

## Movement (physics)

- **All moving actors** (player, enemies, projectiles) use a **kinematic `Rigidbody2D`** configured via [`KinematicBody2D.Configure`](../Assets/Scripts/Physics/KinematicBody2D.cs): no gravity, continuous detection, interpolation.
- **Move only in `FixedUpdate`** with `KinematicBody2D.MoveBy` (`Rigidbody2D.MovePosition`). Never use `transform.Translate` for gameplay motion — it desyncs colliders from the rigidbody.
- **Teleport** (held enemy follow, throw spawn) uses `KinematicBody2D.SetPosition` / `Enemy.SetWorldPosition`, not `transform.position`.
- **`Update`** is for input, animation, timers, and queries — not displacement.

## Object-level setup

| Role | Rigidbody2D | Collider | Tag |
|------|-------------|----------|-----|
| Player | kinematic | solid `BoxCollider2D` | `Player` |
| Enemy | kinematic | trigger `BoxCollider2D` (disabled while held) | `Enemy` |
| Projectile | kinematic | trigger `BoxCollider2D` | untagged |

- **Triggers** = gameplay overlap. **Solid** = blocking only (player body).
- **Damage** targets implement `IDamageable`; respect `InvulnerabilityController` before applying damage.

## Contact detection & handlers

**One primary detection method per interaction.** Pick by who starts it and how fast the mover is:

| | Detection | Entry script |
|---|-----------|--------------|
| Deliberate action (pickup button) | `OverlapCircle` / `Raycast` at action time | initiator (`PlayerController`) |
| Slow passive contact (thrown slime) | `OnTriggerEnter2D` on the **mover** | `Enemy` |
| Fast passive contact (projectile) | `BoxCast` along movement delta each `FixedUpdate` | `SlimeProjectile` |

We currently use four APIs because these cases differ — not because every object needs all four. Pickup is query-based so walking into a slime doesn't auto-pick it up. Thrown enemies are slow enough for triggers. Projectiles outrun trigger gaps between physics steps, so they sweep.

**Handlers** sit on the **target** (the object whose type determines the outcome). They do not detect contact — they override the result.

```text
detect → entry script (defaults) → handler on target? → if false / absent, keep default
```

- `IEnemyPickupHandler`, `IEnemyHitHandler` — per-enemy-type overrides; return `true` to skip `Enemy` defaults.
- `IDamageable` — damage receivers; no handler indirection today.
- New interaction *family* → new interface + entry script. New enemy *variant* → implement existing interface on the prefab; don't branch in `Enemy.cs`.

## Rules of thumb

1. Kinematic RB + `FixedUpdate` + `KinematicBody2D` for all movement.
2. One detection path per interaction; initiated = query, slow contact = trigger on mover, fast contact = sweep.
3. Entry script owns defaults; handlers on the target own type-specific overrides.
