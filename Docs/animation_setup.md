# Player Animation Setup Guide (Unity Animator Controller)

This guide walks you through setting up walking animations for the player character using Unity's Animator Controller system. This approach uses Unity's built-in animation system with a visual state machine.

## Overview

The animation system uses:
- **PlayerAnimator.cs** - Sets Animator parameters based on player state
- **PlayerController.cs** - Already integrated to notify the animator of state changes
- **Unity Animator Controller** - Visual state machine (created in Unity Editor)
- **Animation Clips** - Created from sprite sequences (created in Unity Editor)

## Step-by-Step Setup

### 1. Create Animation Clips

For each direction and state, you need to create Animation Clips from your sprite sequences.

#### Create Animation Clips for Hands Down (Normal Walking):

1. In the Project window, navigate to `Assets/Sprites/`
2. Select the sprite sheet `sprZinkWalkN.png`
3. In the Inspector, verify it's set to "Multiple" sprite mode with 2 sprites: `sprZinkWalkN_0` and `sprZinkWalkN_1`
4. Select both sprites (`sprZinkWalkN_0` and `sprZinkWalkN_1`) in the Project window
5. Drag them into the Scene Hierarchy onto your Player GameObject
6. Unity will prompt you to create an Animation Clip - name it `WalkNorth`
7. Save it in a folder like `Assets/Animations/Player/`
8. Repeat for:
   - `sprZinkWalkS` → `WalkSouth`
   - `sprZinkWalkE` → `WalkEast`
   - `sprZinkWalkW` → `WalkWest`

#### Create Animation Clips for Hands Up (Carrying):

Repeat the process for carry sprites:
- `sprZinkCarryN` → `CarryNorth`
- `sprZinkCarryS` → `CarrySouth`
- `sprZinkCarryE` → `CarryEast`
- `sprZinkCarryW` → `CarryWest`

#### Create Idle Animation Clips (Optional):

If you want dedicated idle animations, create clips using the first frame of each walk animation:
- `IdleNorth` (using `sprZinkWalkN_0`)
- `IdleSouth` (using `sprZinkWalkS_0`)
- `IdleEast` (using `sprZinkWalkE_0`)
- `IdleWest` (using `sprZinkWalkW_0`)

**Note:** If you don't create idle clips, the walk animations will just pause on the first frame when not moving.

### 2. Create the Animator Controller

1. In the Project window, right-click in `Assets/Animations/Player/` (or create the folder)
2. Select **Create > Animator Controller**
3. Name it `PlayerAnimatorController`
4. Double-click it to open the Animator window

### 3. Set Up Animator Parameters

In the Animator window (with `PlayerAnimatorController` open):

1. Click the **Parameters** tab (top left)
2. Add the following parameters:
   - **IsMoving** (Bool) - True when player is moving
   - **IsCarrying** (Bool) - True when player is carrying something
   - **Direction** (Int) - 0=South, 1=North, 2=East, 3=West

### 4. Create Animation States

1. In the Animator window, right-click in empty space
2. Select **Create State > Empty**
3. Name it `IdleSouth` (or use your idle clip if you created one)
4. Repeat to create states for all combinations:
   - `WalkNorth`, `WalkSouth`, `WalkEast`, `WalkWest`
   - `CarryNorth`, `CarrySouth`, `CarryEast`, `CarryWest`
   - `IdleNorth`, `IdleSouth`, `IdleEast`, `IdleWest` (if you created idle clips)

### 5. Assign Animation Clips to States

1. Select each state in the Animator window
2. In the Inspector, assign the corresponding Animation Clip to the **Motion** field
3. For example:
   - `WalkNorth` state → `WalkNorth` clip
   - `CarrySouth` state → `CarrySouth` clip

### 6. Set Up State Machine Logic

You need transitions that send the animator to the right state based on **IsMoving**, **IsCarrying**, and **Direction**. There are two main ways to do that: **Any State** (recommended) or **combinatorial state-to-state** transitions. A third option is **sub-state machines**, which keep "which mode" and "which direction" separate and are often cleaner.

---

#### Option A: Any State (recommended)

**Idea:** Don’t care *where* we are now. Every frame, ask: “Given current parameters, which state *should* we be in?” Then jump there from **Any State**.

- Use the orange **Any State** node.
- Create **one transition from Any State to each of your 12 states** (e.g. Any State → WalkSouth, Any State → IdleNorth, …).
- On each transition, set **conditions** so it only fires when that state is correct:
  - **Any State → WalkSouth**: `IsMoving` true, `IsCarrying` false, `Direction` equals 0.
  - **Any State → IdleNorth**: `IsMoving` false, `Direction` equals 1. (IsCarrying can be true or false for idle; add it if you use different idle when carrying.)
  - …and so on for all 12.

**Total: 12 transitions.** Conditions are mutually exclusive (exactly one combination of IsMoving, IsCarrying, Direction at a time), so only one transition can fire. No need to think “from Idle South go to Walk South”—you never build transitions from one concrete state to another.

**Why not “Idle South → Walk South”?** That would mean we’re building transitions *from every state to every other state*. From IdleSouth we’d need arrows to WalkSouth, WalkNorth, WalkEast, WalkWest, CarrySouth, …, IdleNorth, IdleEast, IdleWest. And the same from WalkSouth, WalkNorth, … So we’d end up with a huge number of arrows (on the order of 12 × 11 = 132) and duplicate condition logic on each. That’s the **combinatorial approach**—it works but is messy and easy to get wrong. **We don’t do that;** we use Any State so “current state” doesn’t matter, only the parameters do.

**How to set conditions (for Any State or any transition):**

1. Click the **transition arrow** in the Animator.
2. In the Inspector, **Conditions**: click **+** to add a condition.
3. Choose **Parameter** (IsMoving / IsCarrying / Direction), then the comparison (true/false for bools, “equals” and a value for Direction).
4. All conditions on one transition are **ANDed**; the transition runs only when all are true.
5. Uncheck **Has Exit Time**, set **Transition Duration** to 0, so the change is instant.

---

#### Option B: Sub-state machines (“if idle then one of these 4”)

**Idea:** Split “mode” (idle / walking / carrying) from “direction” (N/S/E/W). Top level chooses the mode; inside each mode you only switch between the 4 directions.

- **Top level:** Three nodes: **Idle** (sub-state machine), **Walking** (sub-state machine), **Carrying** (sub-state machine).
- **Transitions from Any State** into each sub-state machine:
  - Any State → **Idle** when e.g. `IsMoving` false (and optionally `IsCarrying` false if you want).
  - Any State → **Walking** when `IsMoving` true and `IsCarrying` false.
  - Any State → **Carrying** when `IsMoving` true and `IsCarrying` true.
- **Inside Idle:** Four states — IdleNorth, IdleSouth, IdleEast, IdleWest. Transitions between them **only** use **Direction** (e.g. IdleSouth → IdleNorth when Direction equals 1). So “if we’re idle, which of these 4?” is just Direction inside the Idle sub-state.
- **Inside Walking:** Same idea — WalkNorth, WalkSouth, WalkEast, WalkWest; transitions between them by Direction only.
- **Inside Carrying:** Same — CarryNorth/South/East/West, transitions by Direction only.

So “if idle then check these 4” is exactly what the Idle sub-state machine does: you’re already in “idle mode,” and the only variable is Direction. Fewer conditions per transition, and the graph is easier to read (mode at top, direction inside each box). **Yes, sub-state machines are a cleaner way to handle “if idle then one of these 4.”**

---

#### Summary

| Approach | What you build | Pros / cons |
|----------|----------------|-------------|
| **Any State** | 12 transitions: Any State → each of the 12 states, each with full (IsMoving, IsCarrying, Direction) conditions. | Simple count, no “from which state” logic; a bit repetitive conditions. |
| **Combinatorial** | From every state, a transition to every other state that can be reached (e.g. Idle South → Walk South, Idle South → Walk North, …). | Explodes to many transitions; easy to miss one or duplicate logic. **Not recommended.** |
| **Sub-state machines** | Top level: Idle / Walking / Carrying; inside each, 4 direction states and transitions only by Direction. | Clean separation of mode vs direction; “if idle then these 4” lives in one place. **Cleaner for direction switching.** |

#### How to create sub-state machines (with states under them) in Unity

1. **Open the Animator**
   - Double-click your Animator Controller in the Project window so the **Animator** window is open.

2. **Create a sub-state machine**
   - In the Animator window, **right-click** in empty space.
   - Choose **Create Sub-State Machine**.
   - A new node appears (often labeled "New Sub-State Machine"). Rename it (e.g. **Idle**) by selecting it and changing the name in the Inspector or in the Layers/Parameters area.

3. **Put states inside it**
   - **Double-click** the sub-state machine node. The graph view now shows *inside* that sub-state machine (you'll see something like "Idle" in the breadcrumb at the top of the graph).
   - In this view you're *inside* the sub-state machine. **Right-click** in empty space → **Create State** → **Empty** (or "From New Blend Tree" if you prefer). Create one state per direction, e.g. **IdleSouth**, **IdleNorth**, **IdleEast**, **IdleWest**.
   - Assign each state's **Motion** in the Inspector (the animation clip for that direction).
   - Set the **default state** for this sub-state: right-click the state that should be default (e.g. IdleSouth) → **Set as Layer Default State**.

4. **Add transitions inside the sub-state (direction only)**
   - Still inside the sub-state machine, create transitions between the four states based only on **Direction** (e.g. IdleSouth → IdleNorth when Direction equals 1, IdleSouth → IdleEast when Direction equals 2, etc.). Use **Make Transition** and set conditions on each arrow.

5. **Go back to the parent layer**
   - Click the **breadcrumb** at the top of the Animator graph (e.g. "Base Layer" or "Idle") to go up one level. You should see your sub-state machine as a single node again.

6. **Repeat for other modes**
   - Create another **Create Sub-State Machine** for **Walking**, double-click into it, add WalkNorth / WalkSouth / WalkEast / WalkWest and their Direction-based transitions. Then create one for **Carrying** with CarryNorth/South/East/West.

7. **Wire the top level**
   - On the **base layer** (the top-level graph), add transitions **from Any State** into each sub-state machine node:
     - Any State → **Idle** when e.g. `IsMoving` false.
     - Any State → **Walking** when `IsMoving` true and `IsCarrying` false.
     - Any State → **Carrying** when `IsMoving` true and `IsCarrying` true.

**Summary:** Sub-state machine = one node on the parent layer. **Double-click** that node to go inside it; add states and transitions there. Use the breadcrumb to return to the parent. States "under" a sub-state are just normal states that live inside that sub-state machine's graph.

### 7. Set Default State

1. Right-click `IdleSouth` (or your preferred default)
2. Select **Set as Layer Default State**

### 8. Configure the Player GameObject

1. Select your Player GameObject in the scene
2. Add an **Animator** component (if not already present)
3. Assign `PlayerAnimatorController` to the **Controller** field
4. Add the **PlayerAnimator** component
5. In PlayerAnimator, assign the Animator component to the **Animator** field (or it will auto-detect)

### 9. Configure Animation Clips

For each Animation Clip:
1. Select the clip in the Project window
2. In Inspector, set:
   - **Loop Time**: Checked (for walking animations)
   - **Sample Rate**: Adjust if needed (default is usually fine)

## How It Works

1. **PlayerController** detects movement, direction changes, and carrying state
2. **PlayerAnimator** receives these updates and sets Animator parameters:
   - `IsMoving` (bool)
   - `IsCarrying` (bool)
   - `Direction` (int: 0=South, 1=North, 2=East, 3=West)
3. **Animator Controller** evaluates the parameters and transitions between states
4. **Animation Clips** play the appropriate sprite sequences

## Testing

1. Enter Play Mode
2. Move the player in different directions - walking animations should play
3. Pick up a slime - animations should switch to "hands up" (carry) variants
4. Stop moving - animations should switch to idle
5. Throw the slime - animations should switch back to "hands down" (normal) variants

## Troubleshooting

### Animations not playing?
- Check that the Animator Controller is assigned to the Animator component
- Verify all Animation Clips are assigned to their states
- Check that transitions have correct conditions
- Make sure **Has Exit Time** is unchecked for responsive transitions

### Wrong animations playing?
- Verify parameter names match exactly: `IsMoving`, `IsCarrying`, `Direction`
- Check that Direction values are correct (0=South, 1=North, 2=East, 3=West)
- Review transition conditions in the Animator window

### Animations too fast/slow?
- Select the Animation Clip in Project window
- Adjust the **Sample Rate** in the Inspector
- Or adjust the clip's speed in the Animator state (select state → Inspector → Speed)

### States not transitioning?
- Check that transitions exist between all relevant states
- Verify transition conditions are set correctly
- Make sure **Has Exit Time** is unchecked
- Check that **Transition Duration** is set appropriately (0 for instant)

## Quick Reference: Parameter Values

- **IsMoving**: `true` when moving, `false` when idle
- **IsCarrying**: `true` when holding something, `false` when empty-handed
- **Direction**: 
  - `0` = North
  - `1` = East
  - `2` = South
  - `3` = West

## Alternative: Blend Trees

For smooth blending between directions (e.g. diagonal or rotation), you can use **Blend Trees** instead of separate N/S/E/W states. That’s a different workflow (blend parameters rather than discrete Direction values). The approaches in **§6** (Any State or sub-state machines) are the right place to start for discrete 4-direction sprite animation.
