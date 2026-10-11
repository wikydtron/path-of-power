# Level authoring checks

The [README](../README.md) introduces the level format. This guide covers checks to make before trying an authored level in Unity.

## Files and loading

Authored levels live in [`Assets/StreamingAssets/Levels`](../Assets/StreamingAssets/Levels). [`DungeonLoader.LoadLevelByIndex`](../Assets/Scripts/Dungeon/DungeonLoader.cs) formats an index as `level_01`, `level_02`, and so on, then reads the corresponding JSON file.

[`RunManager`](../Assets/Scripts/Core/RunManager.cs) currently starts at index 1 and completes a normal run after 10 levels. Adding `level_11.json` alone does not extend the normal run. Daily mode generates its first level and uses the indexed files for subsequent levels.

## Check the graph before playtesting

- Give every node a unique integer `id`. [`DungeonGraph.AddNode`](../Assets/Scripts/Dungeon/DungeonGraph.cs) stores nodes by ID; a duplicate replaces an earlier entry.
- Make `startNodeId` identify a node that exists. Run initialization accesses that node to mark it visited.
- Make every connection target an existing node. A connection is directed; adding `1` to node 0 does not create a return connection from node 1.
- Keep a reachable node of type `Boss` on the intended completion path. `RunManager` advances levels after interacting successfully with a `Boss` node; it does not use `bossNodeId` to detect completion.
- Use empty `connections` arrays for terminal nodes and initialize authored nodes with `visited: false`.
- Compare fields with [`LevelData`](../Assets/Scripts/Data/LevelData.cs) and [`NodeData`](../Assets/Scripts/Data/NodeData.cs). Existing level files use names such as `Start`, `Buff`, and `Boss` for node types; verify the loaded values in Unity, not just JSON syntax.

`DungeonLoader` handles file access and parsing errors, but does not validate graph uniqueness, reachability, or balance. A successful JSON parse does not establish that the level is playable.

## Check power and rewards

[`CombatResolver`](../Assets/Scripts/Entities/CombatResolver.cs) applies these rules:

| Node | Effect |
| --- | --- |
| Enemy or Boss | Power must be strictly greater than enemy power to win without a shield. |
| Buff | Adds node power, adjusted by the bonus gain ability. |
| Multiplier | Multiplies current power by node power. |
| Trap below 1 | Subtracts that fraction of current power. For example, `0.25` removes 25%. |
| Trap at least 1 | Subtracts that flat amount, with remaining power clamped to at least `0.1`. |
| Chest | Grants gold from the integer part of node power, adjusted by Greed. |
| Start or Exit | Moves without changing power; `Exit` does not trigger level completion. |

Check at least one winning route with a fresh player's abilities. For example, the left route through [`level_01.json`](../Assets/StreamingAssets/Levels/level_01.json) takes starting power 1 through buffs of 5, 4, and 3, reaching power 13 before the boss at 12. Matching the boss at 12 would lose without a shield.

## Manual verification

1. Work in a separate checkout or copy of the Unity project, and back up existing player data before playing with valuable progress; see [save data](save-data.md).
2. Confirm that the expected level name, start node, and connections render.
3. Check every branch for missing connection targets and unintended dead ends.
4. Follow the intended winning path with a fresh profile, then inspect a losing path and each special node type used.
5. Defeat the boss and confirm that the next indexed level loads, or that level 10 completes the run.
6. Inspect the Unity Console for `[DungeonLoader]` and `[RunManager]` errors.

These are manual checks; this repository does not currently include an automated level validation suite.
