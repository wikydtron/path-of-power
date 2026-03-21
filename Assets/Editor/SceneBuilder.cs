using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

/// <summary>
/// Path of Power — Auto Scene Builder
/// Menu: Tools → Path of Power → Build All Scenes
/// Run this ONCE after opening the project for the first time.
/// </summary>
public class SceneBuilder
{
    private const string SCENES_PATH = "Assets/Scenes";
    private const string PREFABS_PATH = "Assets/Prefabs";

    [MenuItem("Tools/Path of Power/Build All Scenes")]
    public static void BuildAllScenes()
    {
        // Ensure folders exist
        if (!AssetDatabase.IsValidFolder(SCENES_PATH))
            AssetDatabase.CreateFolder("Assets", "Scenes");
        if (!AssetDatabase.IsValidFolder(PREFABS_PATH))
            AssetDatabase.CreateFolder("Assets", "Prefabs");

        CreateBootstrapScene();
        CreateMainMenuScene();
        CreateGameScene();
        CreateUpgradeScreenScene();
        CreatePrefabs();
        SetupBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog(
            "Path of Power",
            "✅ All scenes built successfully!\n\n" +
            "Scenes created:\n" +
            "  • Bootstrap (index 0)\n" +
            "  • MainMenu (index 1)\n" +
            "  • Game (index 2)\n" +
            "  • UpgradeScreen (index 3)\n\n" +
            "Prefabs created:\n" +
            "  • NodePrefab\n" +
            "  • ConnectionLinePrefab\n\n" +
            "Open Bootstrap scene and hit Play!",
            "Let's go!"
        );

        // Open Bootstrap to start
        EditorSceneManager.OpenScene(SCENES_PATH + "/Bootstrap.unity");
    }

    // ─────────────────────────────────────────
    // SCENE 1: Bootstrap
    // ─────────────────────────────────────────
    static void CreateBootstrapScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // GameManager object
        var gmObj = new GameObject("GameManagerObj");
        gmObj.AddComponent<GameManager>();

        // AudioManager (DontDestroyOnLoad)
        var audioObj = new GameObject("AudioManagerObj");
        audioObj.AddComponent<AudioManager>();

        EditorSceneManager.SaveScene(scene, SCENES_PATH + "/Bootstrap.unity");
        Debug.Log("[SceneBuilder] Bootstrap scene created.");
    }

    // ─────────────────────────────────────────
    // SCENE 2: MainMenu
    // ─────────────────────────────────────────
    static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
        cam.orthographic = true;
        camObj.AddComponent<AudioListener>();

        // Canvas
        var canvasObj = CreateCanvas("Canvas");
        var canvas = canvasObj.GetComponent<Canvas>();

        // MainMenuUI controller
        canvasObj.AddComponent<MainMenuUI>();

        // Background panel
        var bg = CreateUIPanel(canvasObj, "Background", new Color(0.05f, 0.05f, 0.15f));
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Title label
        var titleObj = CreateTMPLabel(canvasObj, "TitleLabel", "PATH OF POWER", 60, FontStyle.Bold);
        var titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 160);
        titleRect.sizeDelta = new Vector2(700, 100);

        // Play button
        var playBtn = CreateButton(canvasObj, "PlayButton", "▶  PLAY", new Vector2(0, 40));

        // Daily button
        var dailyBtn = CreateButton(canvasObj, "DailyButton", "📅  DAILY CHALLENGE", new Vector2(0, -60));

        // Upgrades button
        var upgradesBtn = CreateButton(canvasObj, "UpgradesButton", "⚡  UPGRADES", new Vector2(0, -160));

        // Stats label
        var statsLabel = CreateTMPLabel(canvasObj, "StatsLabel", "Runs: 0   Best: 0   Gold: 0", 20, FontStyle.Normal);
        var statsRect = statsLabel.GetComponent<RectTransform>();
        statsRect.anchoredPosition = new Vector2(0, -300);
        statsRect.sizeDelta = new Vector2(600, 40);

        // Version label
        var versionLabel = CreateTMPLabel(canvasObj, "VersionLabel", "v1.0.0", 14, FontStyle.Normal);
        var versionRect = versionLabel.GetComponent<RectTransform>();
        versionRect.anchorMin = new Vector2(1, 0);
        versionRect.anchorMax = new Vector2(1, 0);
        versionRect.pivot = new Vector2(1, 0);
        versionRect.anchoredPosition = new Vector2(-10, 10);
        versionRect.sizeDelta = new Vector2(100, 30);

        // Wire MainMenuUI references via SerializedObject
        var mmUI = canvasObj.GetComponent<MainMenuUI>();
        var so = new SerializedObject(mmUI);
        so.FindProperty("playButton")?.SetValue(playBtn.GetComponent<UnityEngine.UI.Button>());
        so.FindProperty("dailyChallengeButton")?.SetValue(dailyBtn.GetComponent<UnityEngine.UI.Button>());
        so.FindProperty("upgradesButton")?.SetValue(upgradesBtn.GetComponent<UnityEngine.UI.Button>());
        so.FindProperty("statsLabel")?.SetValue(statsLabel.GetComponent<TMPro.TextMeshProUGUI>());
        so.FindProperty("versionLabel")?.SetValue(versionLabel.GetComponent<TMPro.TextMeshProUGUI>());
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, SCENES_PATH + "/MainMenu.unity");
        Debug.Log("[SceneBuilder] MainMenu scene created.");
    }

    // ─────────────────────────────────────────
    // SCENE 3: Game
    // ─────────────────────────────────────────
    static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.03f, 0.03f, 0.08f);
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        camObj.AddComponent<AudioListener>();

        // RunManager
        var runManagerObj = new GameObject("RunManagerObj");
        var runManager = runManagerObj.AddComponent<RunManager>();

        // Player
        var playerObj = new GameObject("PlayerObj");
        var player = playerObj.AddComponent<Player>();

        // UpgradeManager
        var upgradeManagerObj = new GameObject("UpgradeManagerObj");
        var upgradeManager = upgradeManagerObj.AddComponent<UpgradeManager>();

        // DungeonBuilder
        var dungeonBuilderObj = new GameObject("DungeonBuilderObj");
        var dungeonBuilder = dungeonBuilderObj.AddComponent<DungeonBuilder>();

        // Wire RunManager references
        var rmSO = new SerializedObject(runManager);
        rmSO.FindProperty("dungeonBuilder")?.SetValue(dungeonBuilder);
        rmSO.FindProperty("upgradeManager")?.SetValue(upgradeManager);
        rmSO.FindProperty("player")?.SetValue(player);
        rmSO.ApplyModifiedProperties();

        // Canvas (HUD)
        var canvasObj = CreateCanvas("Canvas");

        // HUD
        var hudObj = new GameObject("HUD");
        hudObj.transform.SetParent(canvasObj.transform, false);
        var hudController = hudObj.AddComponent<HUDController>();

        var powerLabel = CreateTMPLabel(canvasObj, "PowerLabel", "⚡ Power: 1", 28, FontStyle.Bold);
        var powerRect = powerLabel.GetComponent<RectTransform>();
        powerRect.anchorMin = new Vector2(0, 1);
        powerRect.anchorMax = new Vector2(0, 1);
        powerRect.pivot = new Vector2(0, 1);
        powerRect.anchoredPosition = new Vector2(20, -20);
        powerRect.sizeDelta = new Vector2(300, 50);

        var goldLabel = CreateTMPLabel(canvasObj, "GoldLabel", "🪙 Gold: 0", 24, FontStyle.Normal);
        var goldRect = goldLabel.GetComponent<RectTransform>();
        goldRect.anchorMin = new Vector2(0, 1);
        goldRect.anchorMax = new Vector2(0, 1);
        goldRect.pivot = new Vector2(0, 1);
        goldRect.anchoredPosition = new Vector2(20, -75);
        goldRect.sizeDelta = new Vector2(250, 40);

        var stageLabel = CreateTMPLabel(canvasObj, "StageLabel", "Stage 1", 22, FontStyle.Normal);
        var stageRect = stageLabel.GetComponent<RectTransform>();
        stageRect.anchorMin = new Vector2(0.5f, 1);
        stageRect.anchorMax = new Vector2(0.5f, 1);
        stageRect.pivot = new Vector2(0.5f, 1);
        stageRect.anchoredPosition = new Vector2(0, -20);
        stageRect.sizeDelta = new Vector2(300, 50);

        var levelNameLabel = CreateTMPLabel(canvasObj, "LevelNameLabel", "The Crypt", 18, FontStyle.Italic);
        var levelNameRect = levelNameLabel.GetComponent<RectTransform>();
        levelNameRect.anchorMin = new Vector2(0.5f, 1);
        levelNameRect.anchorMax = new Vector2(0.5f, 1);
        levelNameRect.pivot = new Vector2(0.5f, 1);
        levelNameRect.anchoredPosition = new Vector2(0, -60);
        levelNameRect.sizeDelta = new Vector2(300, 40);

        // Wire HUD references
        var hudSO = new SerializedObject(hudController);
        hudSO.FindProperty("powerLabel")?.SetValue(powerLabel.GetComponent<TMPro.TextMeshProUGUI>());
        hudSO.FindProperty("goldLabel")?.SetValue(goldLabel.GetComponent<TMPro.TextMeshProUGUI>());
        hudSO.FindProperty("stageLabel")?.SetValue(stageLabel.GetComponent<TMPro.TextMeshProUGUI>());
        hudSO.FindProperty("levelNameLabel")?.SetValue(levelNameLabel.GetComponent<TMPro.TextMeshProUGUI>());
        hudSO.ApplyModifiedProperties();

        // GameOver Panel (inactive by default)
        var gameOverPanel = CreateUIPanel(canvasObj, "GameOverPanel", new Color(0, 0, 0, 0.85f));
        gameOverPanel.SetActive(false);
        var gameOverRect = gameOverPanel.GetComponent<RectTransform>();
        gameOverRect.anchorMin = Vector2.zero;
        gameOverRect.anchorMax = Vector2.one;
        gameOverRect.offsetMin = Vector2.zero;
        gameOverRect.offsetMax = Vector2.zero;

        var gameOverUI = gameOverPanel.AddComponent<GameOverUI>();

        var gameOverTitle = CreateTMPLabel(gameOverPanel, "GameOverTitle", "GAME OVER", 56, FontStyle.Bold);
        var gotRect = gameOverTitle.GetComponent<RectTransform>();
        gotRect.anchoredPosition = new Vector2(0, 80);
        gotRect.sizeDelta = new Vector2(600, 80);

        var gameOverStats = CreateTMPLabel(gameOverPanel, "StatsLabel", "Power reached: 0\nGold earned: 0", 24, FontStyle.Normal);
        var gosRect = gameOverStats.GetComponent<RectTransform>();
        gosRect.anchoredPosition = new Vector2(0, -20);
        gosRect.sizeDelta = new Vector2(500, 80);

        var retryBtn = CreateButton(gameOverPanel, "RetryButton", "↺  TRY AGAIN", new Vector2(0, -120));
        var mainMenuBtn = CreateButton(gameOverPanel, "MainMenuButton", "🏠  MAIN MENU", new Vector2(0, -200));

        // Wire GameOverUI references
        var goSO = new SerializedObject(gameOverUI);
        goSO.FindProperty("gameOverPanel")?.SetValue(gameOverPanel);
        goSO.FindProperty("statsLabel")?.SetValue(gameOverStats.GetComponent<TMPro.TextMeshProUGUI>());
        goSO.FindProperty("retryButton")?.SetValue(retryBtn.GetComponent<UnityEngine.UI.Button>());
        goSO.FindProperty("mainMenuButton")?.SetValue(mainMenuBtn.GetComponent<UnityEngine.UI.Button>());
        goSO.ApplyModifiedProperties();

        // Wire RunManager remaining refs
        var rmSO2 = new SerializedObject(runManager);
        rmSO2.FindProperty("hudController")?.SetValue(hudController);
        rmSO2.FindProperty("gameOverUI")?.SetValue(gameOverUI);
        rmSO2.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, SCENES_PATH + "/Game.unity");
        Debug.Log("[SceneBuilder] Game scene created.");
    }

    // ─────────────────────────────────────────
    // SCENE 4: UpgradeScreen
    // ─────────────────────────────────────────
    static void CreateUpgradeScreenScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
        cam.orthographic = true;
        camObj.AddComponent<AudioListener>();

        // UpgradeManager
        var upgradeManagerObj = new GameObject("UpgradeManagerObj");
        var upgradeManager = upgradeManagerObj.AddComponent<UpgradeManager>();

        // Canvas
        var canvasObj = CreateCanvas("Canvas");

        // Background
        var bg = CreateUIPanel(canvasObj, "Background", new Color(0.05f, 0.05f, 0.15f));
        var bgRect = bg.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // UpgradeMenuUI controller on canvas
        var upgradeMenuUI = canvasObj.AddComponent<UpgradeMenuUI>();

        // Title
        var titleLabel = CreateTMPLabel(canvasObj, "TitleLabel", "⚡ UPGRADES", 48, FontStyle.Bold);
        var titleRect = titleLabel.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1);
        titleRect.anchorMax = new Vector2(0.5f, 1);
        titleRect.pivot = new Vector2(0.5f, 1);
        titleRect.anchoredPosition = new Vector2(0, -30);
        titleRect.sizeDelta = new Vector2(600, 80);

        // Gold label
        var goldLabel = CreateTMPLabel(canvasObj, "GoldLabel", "🪙 Gold: 0", 28, FontStyle.Bold);
        var goldRect = goldLabel.GetComponent<RectTransform>();
        goldRect.anchorMin = new Vector2(0.5f, 1);
        goldRect.anchorMax = new Vector2(0.5f, 1);
        goldRect.pivot = new Vector2(0.5f, 1);
        goldRect.anchoredPosition = new Vector2(0, -110);
        goldRect.sizeDelta = new Vector2(400, 50);

        // Upgrade card container (scroll area)
        var containerObj = new GameObject("UpgradeCardContainer");
        containerObj.transform.SetParent(canvasObj.transform, false);
        var containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.1f, 0.2f);
        containerRect.anchorMax = new Vector2(0.9f, 0.8f);
        containerRect.offsetMin = Vector2.zero;
        containerRect.offsetMax = Vector2.zero;
        var layout = containerObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(10, 10, 10, 10);

        // Start Run button
        var startRunBtn = CreateButton(canvasObj, "StartRunButton", "▶  START RUN", new Vector2(100, -50));
        var startRunRect = startRunBtn.GetComponent<RectTransform>();
        startRunRect.anchorMin = new Vector2(0.5f, 0);
        startRunRect.anchorMax = new Vector2(0.5f, 0);
        startRunRect.pivot = new Vector2(0.5f, 0);
        startRunRect.anchoredPosition = new Vector2(100, 30);
        startRunRect.sizeDelta = new Vector2(200, 60);

        // Main Menu button
        var mainMenuBtn = CreateButton(canvasObj, "MainMenuButton", "🏠  MENU", new Vector2(-100, -50));
        var mainMenuRect = mainMenuBtn.GetComponent<RectTransform>();
        mainMenuRect.anchorMin = new Vector2(0.5f, 0);
        mainMenuRect.anchorMax = new Vector2(0.5f, 0);
        mainMenuRect.pivot = new Vector2(0.5f, 0);
        mainMenuRect.anchoredPosition = new Vector2(-100, 30);
        mainMenuRect.sizeDelta = new Vector2(200, 60);

        // Wire UpgradeMenuUI references
        var umSO = new SerializedObject(upgradeMenuUI);
        umSO.FindProperty("upgradeManager")?.SetValue(upgradeManager);
        umSO.FindProperty("upgradeCardContainer")?.SetValue(containerRect);
        umSO.FindProperty("goldLabel")?.SetValue(goldLabel.GetComponent<TMPro.TextMeshProUGUI>());
        umSO.FindProperty("startRunButton")?.SetValue(startRunBtn.GetComponent<UnityEngine.UI.Button>());
        umSO.FindProperty("mainMenuButton")?.SetValue(mainMenuBtn.GetComponent<UnityEngine.UI.Button>());
        umSO.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, SCENES_PATH + "/UpgradeScreen.unity");
        Debug.Log("[SceneBuilder] UpgradeScreen scene created.");
    }

    // ─────────────────────────────────────────
    // PREFABS
    // ─────────────────────────────────────────
    static void CreatePrefabs()
    {
        // ── NodePrefab ──
        var nodeRoot = new GameObject("NodePrefab");

        // Background circle sprite
        var bgSprite = new GameObject("BG");
        bgSprite.transform.SetParent(nodeRoot.transform, false);
        var bgRenderer = bgSprite.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = CreateCircleSprite();
        bgRenderer.color = new Color(0.2f, 0.2f, 0.3f);
        bgSprite.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        // Glow (available indicator)
        var glowObj = new GameObject("AvailableGlow");
        glowObj.transform.SetParent(nodeRoot.transform, false);
        var glowRenderer = glowObj.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = CreateCircleSprite();
        glowRenderer.color = new Color(1f, 1f, 0f, 0.3f);
        glowObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        glowObj.SetActive(false);

        // Current marker
        var markerObj = new GameObject("CurrentMarker");
        markerObj.transform.SetParent(nodeRoot.transform, false);
        var markerRenderer = markerObj.AddComponent<SpriteRenderer>();
        markerRenderer.sprite = CreateCircleSprite();
        markerRenderer.color = new Color(0f, 1f, 1f, 0.5f);
        markerObj.transform.localScale = new Vector3(1.7f, 1.7f, 1f);
        markerObj.SetActive(false);

        // Power label (TextMeshPro 3D)
        var labelObj = new GameObject("PowerLabel");
        labelObj.transform.SetParent(nodeRoot.transform, false);
        labelObj.transform.localPosition = new Vector3(0, -0.6f, -0.1f);
        var tmp = labelObj.AddComponent<TMPro.TextMeshPro>();
        tmp.text = "0";
        tmp.fontSize = 4;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // Collider for click detection
        var collider = nodeRoot.AddComponent<CircleCollider2D>();
        collider.radius = 0.6f;

        // NodeController script
        var nodeController = nodeRoot.AddComponent<NodeController>();

        // Wire NodeController references
        var ncSO = new SerializedObject(nodeController);
        ncSO.FindProperty("bgRenderer")?.SetValue(bgRenderer);
        ncSO.FindProperty("availableGlow")?.SetValue(glowObj);
        ncSO.FindProperty("currentMarker")?.SetValue(markerObj);
        ncSO.FindProperty("powerLabel")?.SetValue(tmp);
        ncSO.ApplyModifiedProperties();

        // Save NodePrefab
        PrefabUtility.SaveAsPrefabAsset(nodeRoot, PREFABS_PATH + "/NodePrefab.prefab");
        Object.DestroyImmediate(nodeRoot);
        Debug.Log("[SceneBuilder] NodePrefab created.");

        // Wire DungeonBuilder with NodePrefab (it's in a scene — skip for now, user can assign)

        // ── ConnectionLinePrefab ──
        var lineRoot = new GameObject("ConnectionLinePrefab");
        var lineRenderer = lineRoot.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.08f;
        lineRenderer.endWidth = 0.08f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = new Color(0.4f, 0.4f, 0.6f, 0.8f);
        lineRenderer.endColor = new Color(0.4f, 0.4f, 0.6f, 0.8f);

        PrefabUtility.SaveAsPrefabAsset(lineRoot, PREFABS_PATH + "/ConnectionLinePrefab.prefab");
        Object.DestroyImmediate(lineRoot);
        Debug.Log("[SceneBuilder] ConnectionLinePrefab created.");

        // Now wire DungeonBuilder in the Game scene with these prefabs
        WireDungeonBuilderPrefabs();
    }

    static void WireDungeonBuilderPrefabs()
    {
        var gameScenePath = SCENES_PATH + "/Game.unity";
        var scene = EditorSceneManager.OpenScene(gameScenePath, OpenSceneMode.Additive);

        var nodePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS_PATH + "/NodePrefab.prefab");
        var linePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS_PATH + "/ConnectionLinePrefab.prefab");

        foreach (var root in scene.GetRootGameObjects())
        {
            var db = root.GetComponent<DungeonBuilder>();
            if (db != null)
            {
                var so = new SerializedObject(db);
                so.FindProperty("nodePrefab")?.SetValue(nodePrefab);
                so.FindProperty("connectionLinePrefab")?.SetValue(linePrefab);
                so.ApplyModifiedProperties();
                break;
            }
        }

        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.CloseScene(scene, true);
        Debug.Log("[SceneBuilder] DungeonBuilder prefabs wired.");
    }

    // ─────────────────────────────────────────
    // BUILD SETTINGS
    // ─────────────────────────────────────────
    static void SetupBuildSettings()
    {
        var scenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(SCENES_PATH + "/Bootstrap.unity", true),
            new EditorBuildSettingsScene(SCENES_PATH + "/MainMenu.unity", true),
            new EditorBuildSettingsScene(SCENES_PATH + "/Game.unity", true),
            new EditorBuildSettingsScene(SCENES_PATH + "/UpgradeScreen.unity", true),
        };
        EditorBuildSettings.scenes = scenes;
        Debug.Log("[SceneBuilder] Build settings configured (4 scenes).");
    }

    // ─────────────────────────────────────────
    // HELPERS
    // ─────────────────────────────────────────
    static GameObject CreateCanvas(string name)
    {
        var obj = new GameObject(name);
        var canvas = obj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 0;
        var scaler = obj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        obj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        return obj;
    }

    static GameObject CreateUIPanel(GameObject parent, string name, Color color)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        var img = obj.AddComponent<UnityEngine.UI.Image>();
        img.color = color;
        var rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(400, 300);
        return obj;
    }

    static GameObject CreateTMPLabel(GameObject parent, string name, string text, float fontSize, FontStyle style)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        var tmp = obj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.color = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        if (style == FontStyle.Bold) tmp.fontStyle = TMPro.FontStyles.Bold;
        else if (style == FontStyle.Italic) tmp.fontStyle = TMPro.FontStyles.Italic;
        var rect = obj.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 60);
        return obj;
    }

    static GameObject CreateButton(GameObject parent, string name, string label, Vector2 position)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        var img = obj.AddComponent<UnityEngine.UI.Image>();
        img.color = new Color(0.15f, 0.15f, 0.35f);
        var btn = obj.AddComponent<UnityEngine.UI.Button>();

        var colors = btn.colors;
        colors.normalColor = new Color(0.15f, 0.15f, 0.35f);
        colors.highlightedColor = new Color(0.25f, 0.25f, 0.55f);
        colors.pressedColor = new Color(0.1f, 0.1f, 0.25f);
        btn.colors = colors;

        var rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = new Vector2(320, 65);

        // Button text
        var textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        var tmp = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 24;
        tmp.color = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        var textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        return obj;
    }

    static Sprite CreateCircleSprite()
    {
        // Generate a simple circle texture at runtime
        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var pixels = new Color[size * size];
        var center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f - 2;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), center);
                pixels[y * size + x] = dist <= radius ? Color.white : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }
}

// Extension helper to set SerializedProperty object references cleanly
public static class SerializedPropertyExtensions
{
    public static void SetValue(this SerializedProperty prop, Object value)
    {
        if (prop != null)
            prop.objectReferenceValue = value;
    }
}
