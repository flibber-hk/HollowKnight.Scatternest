# HollowKnight.Scatternest
Hollow Knight Randomizer and ItemSync add-on to allow starting at multiple different starting locations.

Notes:
- The number of start locations selected on the connection menu screen will be the number of start locations assigned. It is up to the players to ensure
that this number is less than the number of available starts.
- The randomizer logic will expect that players have access to all of the selected start locations. 
- It is possible to cycle between the available starts, and reset to the original start, using DebugMod (the start must still be
warped to manually through Benchwarp).
- It is possible to choose a fixed start through the normal rando menu; in that case, that start will definitely be assigned, as well as a random selection of other starts.
- If in an ItemSync game, players will be assigned different starts, but will be able to cycle between all of them if
necessary. For instance: 
  - In a 2 player, 2 start game, player A and B will be given starts X and Y.
  - In a 2 player, 3 start game, player A and B will be given starts X and Y. It is possible that progression will require
  starting at start Z, in which case players may need to manually switch their start location.
  - In a 3 player, 2 start game, players A and B will be given start X, and player C will be given start Y.
  - In a 4 player, 2 start game, players A and B will be given start X, and players C and D will be given start Y. (It is impossible for 3 players to be given start X.)
- Due to technical limitations, it may still be necessary for players to warp between each others' starts. For example:
  - Each player might have access to some non-synced progression, but they need to combine their accessible progression to reach a location. This may
  commonly occur if Grubs are not synced - if player A has access to 5 grubs and player B has access to 4 different grubs, then the randomizer will expect
  both players to be able to collect 9 grubs.
  - One player might have to make non-synced changes in the world of the other player. For instance, player A might have access to rescue Bretta, and player
  B might have access to Dirtmouth. The randomizer will expect B to be able to enter the door to Bretta's hut. This type of constraint may be particularly
  common in room randomizer.
  - One player might have access to a bench, and the other player might be expected to equip a charm.
