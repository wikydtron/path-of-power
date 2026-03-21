# 🗡️ Path of Power

**A top-down dungeon puzzle roguelite** — the game those fake mobile ads promised, but for real.

Navigate branching dungeon paths, grow your power number, and defeat bosses. Short runs, big decisions, satisfying number-go-up dopamine.

---

## ⚡ Quick Start

### Prerequisites
- **Unity 2022.3 LTS** (or newer)
- **TextMeshPro** package (ships with Unity — import TMP Essentials when prompted)

### Getting Started
1. Clone the repo: `git clone https://github.com/wikydtron/path-of-power.git`
2. Open in Unity Hub → Add project from disk
3. When prompted, import **TMP Essentials**
4. Open the **Bootstrap** scene (`Assets/Scenes/Bootstrap.unity`)
5. Hit Play

---

## 🎮 How to Play

- **Click** on highlighted nodes to move
- **Grow your power** by collecting buffs (+X), multipliers (×N), and defeating enemies
- **Defeat enemies** by having higher power than them (your power must be > enemy power)
- **Reach and defeat the boss** at the top of each dungeon
- **Earn gold** to buy permanent upgrades between runs
- **Die?** Keep your upgrades and try again!

### Controls
| Input | Action |
|-------|--------|
| Left Click | Move to highlighted node |
| — | That's it. Click to win. |

---

## 🏗️ Architecture

```
┌──────────────┐
│  GameManager │  (Singleton, DontDestroyOnLoad)
│  - State     │  Persists across scenes
│  - PlayerData│  Manages game flow
│  - SaveSystem│
└──────┬───────┘
       │ loads scene
       ▼
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   MainMenu   │ ──► │    Game      │ ──► │ UpgradeScreen│
│  MainMenuUI  │     │  RunManager  │     │ UpgradeMenuUI│
└──────────────┘     │  DungeonBuild│     │ UpgradeManager│
                     │  Player      │     └──────────────┘
                     │  HUDController│
                     │  GameOverUI  │
                     └──────────────┘

Data Flow:
  JSON (StreamingAssets) → DungeonLoader → DungeonGraph → DungeonBuilder (visual)
  Player input → RunManager.TryMoveToNode → CombatResolver → Player state update → HUD update
  
Meta Flow:
  Run end → gold earned → UpgradeManager → AbilitySystem (next run)
```

---

## 📁 Project Structure

```
Assets/
├── Scripts/
│   ├── Core/           GameManager, RunManager, PlayerData, SaveSystem
│   ├── Data/           NodeData, LevelData (pure data classes)
│   ├── Dungeon/        DungeonGraph, DungeonLoader, DungeonBuilder, NodeController
│   ├── Entities/       Player, CombatResolver
│   ├── Upgrades/       UpgradeManager, AbilitySystem
│   ├── UI/             HUDController, GameOverUI, UpgradeMenuUI, MainMenuUI, AudioManager
│   └── Procedural/     LevelGenerator, DailyChallenge
├── StreamingAssets/
│   └── Levels/         level_01.json through level_10.json, upgrades.json
└── Scenes/             Bootstrap, MainMenu, Game, UpgradeScreen
```

---

## 🎬 Scene Setup Guide

### Scene 1: Bootstrap
Create an empty scene named `Bootstrap`. Add a single GameObject:
- **GameManagerObj** — attach `GameManager.cs`
  - On `Start()`, it auto-loads PlayerData and transitions to MainMenu

### Scene 2: MainMenu
- **Canvas** (Screen Space - Overlay)
  - **MainMenuUI** — attach `MainMenuUI.cs`
  - Child buttons: PlayButton, DailyButton, UpgradesButton
  - Child text: StatsLabel (TMP), VersionLabel (TMP)
  - Wire buttons and labels in Inspector

### Scene 3: Game
- **RunManagerObj** — attach `RunManager.cs`
  - Wire references to DungeonBuilder and UpgradeManager
- **PlayerObj** — attach `Player.cs`
- **UpgradeManagerObj** — attach `UpgradeManager.cs`
- **DungeonBuilderObj** — attach `DungeonBuilder.cs`
  - Set NodePrefab and ConnectionLinePrefab references
  - Create a **PlayerToken** sprite (small circle) and assign
- **Canvas** (Screen Space - Overlay)
  - **HUD** — attach `HUDController.cs` with TMP labels for Power, Gold, Stage, LevelName
  - **GameOverPanel** — attach `GameOverUI.cs` (set inactive by default)

### Scene 4: UpgradeScreen
- **UpgradeManagerObj** — attach `UpgradeManager.cs`
- **Canvas**
  - **UpgradeMenuUI** — attach `UpgradeMenuUI.cs`
  - UpgradeCardContainer (Vertical Layout Group)
  - UpgradeCardPrefab — attach `UpgradeCardUI.cs` with TMP labels
  - GoldLabel, StartRunButton, MainMenuButton

### Prefabs to Create

**NodePrefab:**
1. Create a Sprite (Circle) → add `NodeController.cs`
2. Child: BG Sprite (SpriteRenderer for color)
3. Child: PowerLabel (TextMeshPro - 3D)
4. Child: AvailableGlow (sprite, initially inactive)
5. Child: CurrentMarker (sprite, initially inactive)
6. Add a Circle Collider 2D for click detection

**ConnectionLinePrefab:**
1. Empty GameObject → LineRenderer component
2. Material: Default-Line or Sprites-Default
3. Width: 0.08

### AudioManager (Optional)
- Add to Bootstrap scene as DontDestroyOnLoad
- Attach `AudioManager.cs`
- Assign AudioClips in Inspector (or leave null — game works without audio)

### Build Settings
Add scenes in order: Bootstrap (0), MainMenu (1), Game (2), UpgradeScreen (3)

---

## 🗺️ Creating New Levels

Levels are JSON files in `Assets/StreamingAssets/Levels/`.

### JSON Format
```json
{
  "id": "level_11",
  "name": "Your Level Name",
  "biome": "Crypt",
  "startNodeId": 0,
  "bossNodeId": 99,
  "nodes": [
    {
      "id": 0,
      "type": "Start",
      "enemyType": "Basic",
      "power": 0,
      "gridX": 2,
      "gridY": 0,
      "connections": [1, 2],
      "visited": false,
      "displayName": "Start"
    }
  ]
}
```

### Node Types
| Type | Power Meaning | Color |
|------|--------------|-------|
| Start | — | Blue |
| Enemy | Enemy strength (must beat to pass) | Red |
| Buff | Power gained (+X) | Green |
| Multiplier | Power multiplied (×N) | Gold |
| Trap | Power lost (-X flat, or -N% if < 1.0) | Orange |
| Boss | Boss strength (final enemy) | Purple |
| Chest | Gold earned | Yellow |

### Enemy Types
`Basic`, `Tank`, `Mage`, `Boss` — currently cosmetic, affects display name.

### Design Tips
- Player starts with power = 1 (+ upgrades)
- Ensure at least 1 winnable path from start to boss
- `gridY` goes bottom (0) to top — boss should have highest gridY
- `connections` are one-way: list child node IDs that this node leads to

---

## ⚙️ Systems Overview

### Combat System
Deterministic — no RNG in combat. Player power > enemy power = win (absorb enemy power). Player power ≤ enemy power = instant death (unless shield is active). The CombatResolver is a pure static class with zero Unity dependencies.

### Meta Progression
Gold earned during runs (from chests + completion bonus) is spent on permanent upgrades. Upgrades persist across runs via SaveSystem (JSON file in Application.persistentDataPath). Six upgrades available: Starting Power, Bonus Gains, Iron Will (shield), Overkill, Vampirism, Greed.

### Level Loading
Levels are loaded from StreamingAssets as JSON, parsed into LevelData, then converted into a DungeonGraph (adjacency structure). DungeonBuilder renders the graph as GameObjects with clickable nodes.

### Procedural Generation
LevelGenerator creates random-but-validated levels using seeded RNG. It generates layered node graphs with configurable difficulty, then verifies winnability via DFS pathfinding. If no valid path exists, it injects a buff chain.

### Daily Challenge
Uses a deterministic seed from today's UTC date so all players get the same dungeon. Difficulty scales with the day of year (1-10 cycling). Daily scores are tracked separately.

### Audio
AudioManager is a singleton with SFX and music sources. All clips are optional — the game functions without audio assets. Hook up clips in the Inspector when you have them.

---

## 💰 Monetization (Steam)

- **Base price:** $4.99–$9.99
- **No pay-to-win** — all upgrades are earned through gameplay
- **DLC potential:**
  - New biomes (Forest, Volcano, Abyss) with themed enemies
  - New mechanics (status effects, allies, equipment)
  - Cosmetic skins for the player token and nodes
- **Daily Challenges** drive retention without monetization

---

## 📈 Retention Design

- **"One more run" loop** — runs last 5-10 minutes
- **Numbers go up fast** — immediate visual feedback (power counter animates)
- **Daily challenges** — same map for everyone, compete on score
- **Meta progression** — permanent upgrades give a reason to keep playing
- **Risk/reward choices** — every path branch is a meaningful decision

---

## 🧪 Current MVP Scope

- ✅ 10 handcrafted levels (Crypt biome)
- ✅ 1 boss per level
- ✅ 6 permanent upgrades
- ✅ Save system
- ✅ Procedural level generator
- ✅ Daily challenge mode
- ✅ Full combat resolution
- ✅ Animated HUD (power counter lerp + flash)
- ✅ Game over screen with stats
- ✅ Upgrade shop between runs

---

## 🚀 Future Ideas

- Multiple biomes with unique enemy behaviors
- Equipment/inventory system
- Branching world map (choose which dungeon to enter)
- Steam leaderboards for daily challenges
- Achievement system
- Boss special abilities (AOE damage, shields, summons)
- Particle effects and screen shake
- Controller support

---

## 📄 License

All Rights Reserved. © 2026 The Boss Factory.
