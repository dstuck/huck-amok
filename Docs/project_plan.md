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
- [x] grabbing a medium slime will split it into 1 in your hands and another as the remainder (1 small one for now)
- [x] hitting a medium slime with another will reduce it to a small slime
- [x] hitting or grabbing a slime will trigger a invulnerable frames (visualized by flashing) so you can't immediately throw the slime at it [this will be shared behavior with player soon]
- [x] medium slimes will shoot short distance slime balls at the player
    - while in detection radius moves toward player
    - while in firing radius flickers, shoots a slime ball, repeats after a delay

0.4 Damage
- [ ] player has max life with UI
- [ ] damaged by projectiles
- [ ] add invulnerable frames

0.4 Enemy combination
- [ ] single slimes within a certain distance will combine into medium slime

