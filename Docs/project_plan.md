# Overview

Pickup and throw monsters at each other. Monsters are made of individual small components
(maybe slimes or rocks?) that combine into creatures that can attack you. You pick up the
small ones or pull off pieces from a big one to throw and destroy them. Challenge comes from having many different ones on screen at a time since individuals are easily avoided

# Controls
Move - moves character in north/south/east/west direction
Pickup/throw - if holding something, throws it in current direction, otherwise if an enemy is in front of you, pick them up

## Throwing implementation
Enemies need to have active and inactive state. When picked up, they need to enter inactive mode
and be held above the character. When thrown they should remain inactive while fly forward until they've travelled their throw distance. During this thrown state, they should destroy one slime worth of enemy they run into. Then after half a second if they haven't hit anything they should return to active state.

#POC
0.1 Basic controls
- [x] Able to pick up enemy and throw it at another to destroy them both

0.2 Animation
- [x] change direction and carry slimes
- [x] slimes animate while moving

0.3 Stacked Enemies
- [x] grabbing a tier-2 slime will split it into one tier-1 slime in your hands and another as the remainder
- [x] hitting a tier-2 slime with another will reduce it to a tier-1 slime
- [x] hitting or grabbing a slime will trigger a invulnerable frames (visualized by flashing) so you can't immediately throw the slime at it [this will be shared behavior with player soon]
- [x] tier-2 slimes will shoot short distance slime balls at the player
    - while in detection radius moves toward player
    - while in firing radius flickers, shoots a slime ball, repeats after a delay

0.4 Damage
- [x] player has 3 hearts with UI display
- [x] damaged by projectiles
- [x] projectiles immediately splat when they hit the player
- [x] add invulnerable frames
- [x] add sound for shots, hit
- [x] add gameover state when all three hearts are lost that restarts at the beginning
- [x] Add success state when all slimes are defeated

0.5 Enemy combination
- [x] tier-1 slimes within a certain distance combine into tier-2 slime
- [x] add tier-3 slime that shoots faster and splits into tier-1 held + tier-2 remainder

0.6 Enemy types
- [x] Create slime attack types
    - [x] sticky shot (orange) - sticks around for 5s on splat and slows down player movement
    - [x] multi shot (purple) - shots out two projectiles at 20% angle
- [x] Refactor slime colors
    - projectile and single slimes should be based on color type
    - tier 2 and 3 slimes need to track their component types and combine their powers
    - tier 2 and 3 need to mix the colors of the components somehow (either simple gradient or ideally a texturing effect that patterns them together)
- [x] Higher-tier slime colors set by components

0.7 Levels
- [ ] Add boundaries to game
- [ ] Add background and objects
- [ ] Fix camera size for webGL
- [ ] On victory progress to harder level
- [ ] On failure start at level 1
