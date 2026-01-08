# Overview

Pickup and throw monsters at each other. Monsters are made of individual small components
(maybe slimes or rocks?) that combine into creatures that can attack you. You pick up the
small ones or pull off pieces from a big one to throw and destroy them. Challenge comes from having many different ones on screen at a time since individuals are easily avoided

# Controls
Move - moves character in north/south/east/west direction
Pickup/throw - if holding something, throws it in current direction, otherwise if an enemy is in front of you, pick them up

#POC
0.1 Able to pick up enemy and throw it at another to destroy them both
0.2 Enemies can combine into bigger versions that can attack and damage you
0.3 You can pluck a piece off a bigger version to make it shrink back and give you something to throw

## Throwing implementation
Enemies need to have active and inactive state. When picked up, they need to enter inactive mode
and be held above the character. When thrown they should remain inactive while fly forward until they've travelled their throw distance. During this thrown state, they should destroy one slime worth of enemy they run into. Then after half a second if they haven't hit anything they should return to active state.