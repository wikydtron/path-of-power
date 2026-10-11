# Scene builder: generated assets and remaining setup

[SceneBuilder.cs](../Assets/Editor/SceneBuilder.cs) provides the Unity menu action
**Tools → Path of Power → Build All Scenes**. It creates a starting scene layout;
the resulting project still needs inspection and additional wiring.

## Prepare a disposable project

The repository snapshot contains scripts and level JSON, but no tracked
`ProjectSettings/`, `Packages/manifest.json`, `.unity` scenes, or `.prefab` files.
The Bootstrap path in the README therefore does not identify a checked-in scene.
The [README prerequisites](../README.md) recommend Unity 2022.3 LTS
or newer and TextMeshPro, including TMP Essentials; no exact project/package
configuration is supplied in the repository.

Use a new disposable Unity project or a disposable copy of a project for the
first generation attempt. Copy the repository's `Assets/` content into that
sandbox, retaining its directory structure, and resolve package/import/compiler
errors there before using the menu action. Keep valuable project files and
unsaved scene work outside this experiment.

## What the menu action changes

The builder creates `Assets/Scenes` and `Assets/Prefabs` if they are absent, then
creates four scenes, creates two prefabs, wires the dungeon prefab references,
replaces the build scene list, saves assets, and finally opens Bootstrap.

| Saved path | Main generated content |
| --- | --- |
| `Assets/Scenes/Bootstrap.unity` | GameManager and AudioManager objects. |
| `Assets/Scenes/MainMenu.unity` | Camera, canvas, menu buttons, and labels. |
| `Assets/Scenes/Game.unity` | Camera, RunManager, DungeonBuilder, UpgradeManager, HUD, and initially inactive game-over panel. |
| `Assets/Scenes/UpgradeScreen.unity` | Camera, UpgradeManager, canvas, upgrade container, labels, and navigation buttons. |
| `Assets/Prefabs/NodePrefab.prefab` | Sprite renderers, label, CircleCollider2D, and NodeController. |
| `Assets/Prefabs/ConnectionLinePrefab.prefab` | LineRenderer and a generated material. |

These are fixed save paths. Running the command again can overwrite assets at
those paths; it does not merge your scene or prefab edits. Each scene begins
with `NewSceneMode.Single`, and the builder does not explicitly prompt to save
your existing scene work or offer an overwrite preview.

`SetupBuildSettings` assigns a new `EditorBuildSettings.scenes` array, replacing
the previous list with four enabled entries in this order: **Bootstrap →
MainMenu → Game → UpgradeScreen**. Review that list before any player build.

## Inspect wiring before trying to play

- Game is saved before the prefabs are created. `WireDungeonBuilderPrefabs`
  reopens Game additively, finds the first root DungeonBuilder, assigns the
  NodeController and LineRenderer components from the generated prefabs, and
  saves Game again. Verify both assignments in the Inspector.
- [DungeonBuilder](../Assets/Scripts/Dungeon/DungeonBuilder.cs) also exposes
  `_playerToken`; the builder does not create or assign it. Its movement code
  skips token positioning when this reference is absent.
- The Game scene builder does not create a Player component. Inspect the
  [RunManager's use of Player.Instance](../Assets/Scripts/Core/RunManager.cs)
  and the README's manual scene setup before testing movement or combat.
- The builder creates the upgrade container but no upgrade-card prefab and
  does not assign `_upgradeCardPrefab`. [UpgradeMenuUI](../Assets/Scripts/UI/UpgradeMenuUI.cs)
  returns without populating cards while that reference is absent.
- No EventSystem is created by this script. Check UI input setup in the sandbox.
- AudioManager is added without audio sources or clips. Inspect optional audio
  setup separately, along with generated sprite/material references after reopening.

The completion dialog reports that generation reached its end; it does not
verify gameplay, UI input, or missing references. This guide was checked against
source code only. Scene generation and gameplay have not been run in Unity as
part of this documentation change.
