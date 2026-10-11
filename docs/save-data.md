# Save data and preserving progress

[`SaveSystem`](../Assets/Scripts/Core/SaveSystem.cs) stores progress in a JSON file named `playerdata.json` under Unity's `Application.persistentDataPath`. The directory depends on the platform and Unity project identity; the source does not specify a single fixed filesystem path.

## What is saved

[`PlayerData`](../Assets/Scripts/Core/PlayerData.cs) contains:

| Fields | Purpose |
| --- | --- |
| `totalGold` | Currency available for permanent upgrades. |
| `totalRuns`, `totalWins` | Run statistics. |
| `highScore` | Highest recorded power. |
| `unlockedUpgrades` | Unlocked upgrade IDs. |
| `upgradeLevelKeys`, `upgradeLevelValues` | Parallel lists associating upgrade IDs with levels. |
| `lastDailyDate`, `dailyBestScore` | Daily challenge date and best score fields. |

The two upgrade level lists must stay aligned: an ID at position `i` corresponds to the level at position `i`. `GetUpgradeLevel` reads the value at the matching key index without a separate bounds check. Avoid editing one list independently.

This structure does not include the current dungeon, player node, or in-progress run state. Loading the save is not a resume operation for an interrupted run.

## When data is read and written

[`GameManager.Awake`](../Assets/Scripts/Core/GameManager.cs) loads player data. `SaveGame` writes it through `SaveSystem.Save`; callers include run completion, spending gold, and recording a new high score.

`SaveSystem.Save` serializes with `JsonUtility.ToJson` and writes directly to the save path. It does not create a backup, temporary replacement file, or versioned history.

If the file is missing, loading returns new player data. A read or parse exception also returns new data and logs `[SaveSystem] Failed to load save`. A missing or unreadable file can therefore look like lost progress even though the original file has not been explicitly deleted by `Load`.

## Back up before troubleshooting

1. Stop Play mode or close the game so it cannot write while you copy the file.
2. Locate the active project's `Application.persistentDataPath`. Do not assume another Unity project or build uses the same directory.
3. Copy `playerdata.json` to a separate backup location. Keep the original bytes and a dated copy before trying repairs.
4. Inspect a copy first. Check JSON syntax, expected fields, and matching lengths for the upgrade key/value lists.
5. If you choose to restore a known-good copy, keep the current file as another backup before replacing it while the game is stopped.
6. Relaunch and verify gold, upgrades, and statistics. Check the Unity Console for save/load errors before continuing play.

Avoid calling `SaveSystem.DeleteSave` during diagnostics: it deletes the active save file. Continuing to play after a failed load may write fresh player data to that same path, so preserve the existing file first.

This guide describes current source behavior. It does not change the save format or add automatic recovery.
