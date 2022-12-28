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

## Start Location Exclusion

The Start Location Exclusion page allows players to exclude certain starts from randomization. Scatternest must be enabled to make use of this feature.

Notes:
- Any red starts in the exclusion page will not be selected.
- If using a fixed start in the Randomizer which has been excluded in the exclusion page, the Randomizer
will assign King's Pass, even if that has also been excluded.
- There are several in-built presets for starts to exclude.
  - The Apply Preset Now selector will toggle all of the start buttons to reflect the preset.
  - The Apply Preset On Start selector will apply the preset at the start of randomization, in addition to any already disabled starts.
- The included presets include:
  - Exclude Equivalent Starts - this will randomly select one start from each group of starts that provide equivalent access
  (e.g. West/East Crossroads and Ancestral Mound) and exclude the rest.
  - Exclude Similar Starts - this will randomly select one start from each group of starts that provide similar access 
  (e.g. West/East Crossroads, Ancestral Mound, King's Pass and West Blue Lake) and exclude the rest.
  - Exclude Item Rando Starts - this will exclude starts which are available with the default settings. This will cause
  generation failure unless transitions are randomized or non-default skip or novelty settings are enabled.
- The presets are blind to any changes made by connections. For example, Greenpath and Queen's Station will always be considered equivalent
even if a connection which blocks access between the two is enabled.
- Starts added by connections may or may not appear on this screen, but they will never be excluded by any of the presets.