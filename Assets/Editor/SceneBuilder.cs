using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using PathOfPower.Core;
using PathOfPower.UI;
using PathOfPower.Dungeon;
using PathOfPower.Entities;
using PathOfPower.Upgrades;

/// <summary>
/// Path of Power — Auto Scene Builder
/// Menu: Tools → Path of Power → Build All Scenes
/// Run this ONCE after opening the project for the first time.
/// </summary>
public class SceneBuilder
{
    private const string SCENES_PATH  = "Assets/Scenes";
    private const string PREFABS_PATH = "Assets/Prefabs";

    [MenuItem("Tools/Path of Power/Build All Scenes")]
    public static void BuildAllScenes()
    {
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
            "All scenes built!\n\nOpen Bootstrap and hit Play.",
            "Let's go!"
        );

        EditorSceneManager.OpenScene(SCENES_PATH + "/Bootstrap.unity");
    }

    // ── Bootstrap ──────────────────────────────────────────────────────────
    static void CreateBootstrapScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var gmObj = new GameObject("GameManagerObj");
        gmObj.AddComponent<GameManager>();

        var audioObj = new GameObject("AudioManagerObj");
        audioObj.AddComponent<AudioManager>();

        EditorSceneManager.SaveScene(scene, SCENES_PATH + "/Bootstrap.unity");
    }

    // ── MainMenu ───────────────────────────────────────────────────────────
    static void CreateMainMenuScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
        cam.orthographic = true;
        camObj.AddComponent<AudioListener>();

        var canvasObj = CreateCanvas("Canvas");
        var mmUI = canvasObj.AddComponent<MainMenuUI>();

        var bg = CreateUIPanel(canvasObj, "Background", new Color(0.05f, 0.05f, 0.15f));
        StretchFull(bg);

        var titleObj  = CreateTMPLabel(canvasObj, "TitleLabel", "PATH OF POWER", 60, true);
        SetRect(titleObj, 0, 160, 700, 100);

        var playBtn     = CreateButton(canvasObj, "PlayButton",     "PLAY",            new Vector2(0,   40));
        var dailyBtn    = CreateButton(canvasObj, "DailyButton",    "DAILY CHALLENGE", new Vector2(0,  -60));
        var upgradesBtn = CreateButton(canvasObj, "UpgradesButton", "UPGRADES",        new Vector2(0, -160));

        var statsLabel   = CreateTMPLabel(canvasObj, "StatsLabel",   "Runs: 0  |  Wins: 0  |  Best: 0", 20, false);
        var versionLabel = CreateTMPLabel(canvasObj, "VersionLabel", "v0.1.0", 14, false);
        SetRect(statsLabel, 0, -300, 600, 40);

        var vRect = versionLabel.GetComponent<RectTransform>();
        vRect.anchorMin = new Vector2(1, 0); vRect.anchorMax = new Vector2(1, 0);
        vRect.pivot = new Vector2(1, 0);
        vRect.anchoredPosition = new Vector2(-10, 10);
        vRect.sizeDelta = new Vector2(120, 30);

        var so = new SerializedObject(mmUI);
        so.FindProperty("_playButton")?.SetObjRef(playBtn.GetComponent<UnityEngine.UI.Button>());
        so.FindProperty("_dailyButton")?.SetObjRef(dailyBtn.GetComponent<UnityEngine.UI.Button>());
        so.FindProperty("_upgradesButton")?.SetObjRef(upgradesBtn.GetComponent<UnityEngine.UI.Button>());
        so.FindProperty("_statsLabel")?.SetObjRef(statsLabel.GetComponent<TMPro.TextMeshProUGUI>());
        so.FindProperty("_versionLabel")?.SetObjRef(versionLabel.GetComponent<TMPro.TextMeshProUGUI>());
        so.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, SCENES_PATH + "/MainMenu.unity");
    }

    // ── Game ───────────────────────────────────────────────────────────────
    static void CreateGameScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.03f, 0.03f, 0.08f);
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        camObj.AddComponent<AudioListener>();

        var runManagerObj    = new GameObject("RunManagerObj");
        var runManager       = runManagerObj.AddComponent<RunManager>();

        var dungeonBuilderObj = new GameObject("DungeonBuilderObj");
        var dungeonBuilder    = dungeonBuilderObj.AddComponent<DungeonBuilder>();

        var upgradeManagerObj = new GameObject("UpgradeManagerObj");
        var upgradeManager    = upgradeManagerObj.AddComponent<UpgradeManager>();

        // Wire RunManager
        var rmSO = new SerializedObject(runManager);
        rmSO.FindProperty("_dungeonBuilder")?.SetObjRef(dungeonBuilder);
        rmSO.FindProperty("_upgradeManager")?.SetObjRef(upgradeManager);
        rmSO.ApplyModifiedProperties();

        // Canvas + HUD
        var canvasObj     = CreateCanvas("Canvas");
        var hudObj        = new GameObject("HUD");
        hudObj.transform.SetParent(canvasObj.transform, false);
        var hudController = hudObj.AddComponent<HUDController>();

        var powerLabel     = CreateTMPLabel(canvasObj, "PowerLabel",     "Power: 1",   28, true);
        var goldLabel      = CreateTMPLabel(canvasObj, "GoldLabel",      "Gold: 0",    24, false);
        var stageLabel     = CreateTMPLabel(canvasObj, "StageLabel",     "Stage 1",    22, false);
        var levelNameLabel = CreateTMPLabel(canvasObj, "LevelNameLabel", "The Crypt",  18, false);

        AnchorTopLeft(powerLabel,     20, -20,  300, 50);
        AnchorTopLeft(goldLabel,      20, -75,  250, 40);
        AnchorTopCenter(stageLabel,    0, -20,  300, 50);
        AnchorTopCenter(levelNameLabel,0, -60,  300, 40);

        var hudSO = new SerializedObject(hudController);
        hudSO.FindProperty("_powerLabel")?.SetObjRef(powerLabel.GetComponent<TMPro.TextMeshProUGUI>());
        hudSO.FindProperty("_goldLabel")?.SetObjRef(goldLabel.GetComponent<TMPro.TextMeshProUGUI>());
        hudSO.FindProperty("_stageLabel")?.SetObjRef(stageLabel.GetComponent<TMPro.TextMeshProUGUI>());
        hudSO.FindProperty("_levelNameLabel")?.SetObjRef(levelNameLabel.GetComponent<TMPro.TextMeshProUGUI>());
        hudSO.ApplyModifiedProperties();

        // GameOver panel
        var goPanel = CreateUIPanel(canvasObj, "GameOverPanel", new Color(0, 0, 0, 0.88f));
        goPanel.SetActive(false);
        StretchFull(goPanel);
        var gameOverUI = goPanel.AddComponent<GameOverUI>();

        var goHeader = CreateTMPLabel(goPanel, "HeaderLabel", "GAME OVER", 56, true);
        SetRect(goHeader, 0, 80, 600, 80);

        var goPower = CreateTMPLabel(goPanel, "PowerLabel", "Power: 0", 28, false);
        SetRect(goPower, 0, -10, 400, 50);

        var goGold = CreateTMPLabel(goPanel, "GoldLabel", "Gold earned: 0", 24, false);
        SetRect(goGold, 0, -60, 400, 50);

        var continueBtn = CreateButton(goPanel, "ContinueButton", "CONTINUE",  new Vector2(0, -130));
        var restartBtn  = CreateButton(goPanel, "RestartButton",  "TRY AGAIN", new Vector2(0, -210));

        var goSO = new SerializedObject(gameOverUI);
        goSO.FindProperty("_panel")?.SetObjRef(goPanel);
        goSO.FindProperty("_headerLabel")?.SetObjRef(goHeader.GetComponent<TMPro.TextMeshProUGUI>());
        goSO.FindProperty("_powerLabel")?.SetObjRef(goPower.GetComponent<TMPro.TextMeshProUGUI>());
        goSO.FindProperty("_goldLabel")?.SetObjRef(goGold.GetComponent<TMPro.TextMeshProUGUI>());
        goSO.FindProperty("_continueButton")?.SetObjRef(continueBtn.GetComponent<UnityEngine.UI.Button>());
        goSO.FindProperty("_restartButton")?.SetObjRef(restartBtn.GetComponent<UnityEngine.UI.Button>());
        goSO.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, SCENES_PATH + "/Game.unity");
    }

    // ── UpgradeScreen ──────────────────────────────────────────────────────
    static void CreateUpgradeScreenScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
        cam.orthographic = true;
        camObj.AddComponent<AudioListener>();

        var upgradeManagerObj = new GameObject("UpgradeManagerObj");
        var upgradeManager    = upgradeManagerObj.AddComponent<UpgradeManager>();

        var canvasObj = CreateCanvas("Canvas");
        var umUI = canvasObj.AddComponent<UpgradeMenuUI>();

        var bg = CreateUIPanel(canvasObj, "Background", new Color(0.05f, 0.05f, 0.15f));
        StretchFull(bg);

        var titleLabel = CreateTMPLabel(canvasObj, "TitleLabel", "UPGRADES", 48, true);
        AnchorTopCenter(titleLabel, 0, -30, 600, 80);

        var goldLabel = CreateTMPLabel(canvasObj, "GoldLabel", "Gold: 0", 28, true);
        AnchorTopCenter(goldLabel, 0, -110, 400, 50);

        var containerObj  = new GameObject("UpgradeCardContainer");
        containerObj.transform.SetParent(canvasObj.transform, false);
        var containerRect = containerObj.AddComponent<RectTransform>();
        containerRect.anchorMin = new Vector2(0.1f, 0.2f);
        containerRect.anchorMax = new Vector2(0.9f, 0.8f);
        containerRect.offsetMin = containerRect.offsetMax = Vector2.zero;
        var layout = containerObj.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
        layout.spacing = 12;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childForceExpandWidth  = true;
        layout.childForceExpandHeight = false;
        layout.padding = new RectOffset(10, 10, 10, 10);

        var startRunBtn = CreateButton(canvasObj, "StartRunButton", "START RUN", Vector2.zero);
        var mainMenuBtn = CreateButton(canvasObj, "MainMenuButton", "MAIN MENU", Vector2.zero);
        AnchorBottom(startRunBtn,  100, 30, 200, 60);
        AnchorBottom(mainMenuBtn, -100, 30, 200, 60);

        var umSO = new SerializedObject(umUI);
        umSO.FindProperty("_upgradeCardContainer")?.SetObjRef(containerObj.transform);
        umSO.FindProperty("_goldLabel")?.SetObjRef(goldLabel.GetComponent<TMPro.TextMeshProUGUI>());
        umSO.FindProperty("_startRunButton")?.SetObjRef(startRunBtn.GetComponent<UnityEngine.UI.Button>());
        umSO.FindProperty("_mainMenuButton")?.SetObjRef(mainMenuBtn.GetComponent<UnityEngine.UI.Button>());
        umSO.ApplyModifiedProperties();

        EditorSceneManager.SaveScene(scene, SCENES_PATH + "/UpgradeScreen.unity");
    }

    // ── Prefabs ────────────────────────────────────────────────────────────
    static void CreatePrefabs()
    {
        // NodePrefab
        var nodeRoot = new GameObject("NodePrefab");

        var bgObj = new GameObject("BG");
        bgObj.transform.SetParent(nodeRoot.transform, false);
        var bgRenderer = bgObj.AddComponent<SpriteRenderer>();
        bgRenderer.sprite = CreateCircleSprite();
        bgRenderer.color  = new Color(0.2f, 0.2f, 0.3f);
        bgObj.transform.localScale = new Vector3(1.2f, 1.2f, 1f);

        var glowObj = new GameObject("AvailableGlow");
        glowObj.transform.SetParent(nodeRoot.transform, false);
        var glowRenderer = glowObj.AddComponent<SpriteRenderer>();
        glowRenderer.sprite = CreateCircleSprite();
        glowRenderer.color  = new Color(1f, 1f, 0f, 0.3f);
        glowObj.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        glowObj.SetActive(false);

        var markerObj = new GameObject("CurrentMarker");
        markerObj.transform.SetParent(nodeRoot.transform, false);
        var markerRenderer = markerObj.AddComponent<SpriteRenderer>();
        markerRenderer.sprite = CreateCircleSprite();
        markerRenderer.color  = new Color(0f, 1f, 1f, 0.5f);
        markerObj.transform.localScale = new Vector3(1.7f, 1.7f, 1f);
        markerObj.SetActive(false);

        var labelObj = new GameObject("PowerLabel");
        labelObj.transform.SetParent(nodeRoot.transform, false);
        labelObj.transform.localPosition = new Vector3(0, -0.6f, -0.1f);
        var tmp = labelObj.AddComponent<TMPro.TextMeshPro>();
        tmp.text      = "0";
        tmp.fontSize  = 4;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.color     = Color.white;

        nodeRoot.AddComponent<CircleCollider2D>().radius = 0.6f;
        var nc = nodeRoot.AddComponent<NodeController>();

        var ncSO = new SerializedObject(nc);
        ncSO.FindProperty("_bgRenderer")?.SetObjRef(bgRenderer);
        ncSO.FindProperty("_availableGlow")?.SetObjRef(glowObj);
        ncSO.FindProperty("_currentMarker")?.SetObjRef(markerObj);
        ncSO.FindProperty("_powerLabel")?.SetObjRef(tmp);
        ncSO.ApplyModifiedProperties();

        PrefabUtility.SaveAsPrefabAsset(nodeRoot, PREFABS_PATH + "/NodePrefab.prefab");
        Object.DestroyImmediate(nodeRoot);

        // ConnectionLinePrefab
        var lineRoot = new GameObject("ConnectionLinePrefab");
        var lr = lineRoot.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.startWidth = lr.endWidth = 0.08f;
        lr.useWorldSpace = true;
        lr.material      = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = lr.endColor = new Color(0.4f, 0.4f, 0.6f, 0.8f);
        PrefabUtility.SaveAsPrefabAsset(lineRoot, PREFABS_PATH + "/ConnectionLinePrefab.prefab");
        Object.DestroyImmediate(lineRoot);

        WireDungeonBuilderPrefabs();
    }

    static void WireDungeonBuilderPrefabs()
    {
        var scene      = EditorSceneManager.OpenScene(SCENES_PATH + "/Game.unity", OpenSceneMode.Additive);
        var nodePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS_PATH + "/NodePrefab.prefab");
        var linePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PREFABS_PATH + "/ConnectionLinePrefab.prefab");

        foreach (var root in scene.GetRootGameObjects())
        {
            var db = root.GetComponent<DungeonBuilder>();
            if (db == null) continue;
            var so = new SerializedObject(db);
            // DungeonBuilder expects NodeController prefab, not GameObject
            so.FindProperty("_nodePrefab")?.SetObjRef(nodePrefab.GetComponent<NodeController>());
            so.FindProperty("_connectionLinePrefab")?.SetObjRef(linePrefab.GetComponent<LineRenderer>());
            so.ApplyModifiedProperties();
            break;
        }

        EditorSceneManager.SaveScene(scene);
        EditorSceneManager.CloseScene(scene, true);
    }

    // ── Build Settings ─────────────────────────────────────────────────────
    static void SetupBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(SCENES_PATH + "/Bootstrap.unity",     true),
            new EditorBuildSettingsScene(SCENES_PATH + "/MainMenu.unity",      true),
            new EditorBuildSettingsScene(SCENES_PATH + "/Game.unity",          true),
            new EditorBuildSettingsScene(SCENES_PATH + "/UpgradeScreen.unity", true),
        };
    }

    // ── Helpers ────────────────────────────────────────────────────────────
    static GameObject CreateCanvas(string name)
    {
        var obj    = new GameObject(name);
        var canvas = obj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = obj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode          = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight   = 0.5f;
        obj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        return obj;
    }

    static GameObject CreateUIPanel(GameObject parent, string name, Color color)
    {
        var obj  = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        obj.AddComponent<UnityEngine.UI.Image>().color = color;
        return obj;
    }

    static GameObject CreateTMPLabel(GameObject parent, string name, string text, float size, bool bold)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        var tmp       = obj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.color     = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        if (bold) tmp.fontStyle = TMPro.FontStyles.Bold;
        obj.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 60);
        return obj;
    }

    static GameObject CreateButton(GameObject parent, string name, string label, Vector2 pos)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent.transform, false);
        obj.AddComponent<UnityEngine.UI.Image>().color = new Color(0.15f, 0.15f, 0.35f);
        var btn = obj.AddComponent<UnityEngine.UI.Button>();
        var c   = btn.colors;
        c.highlightedColor = new Color(0.25f, 0.25f, 0.55f);
        c.pressedColor     = new Color(0.10f, 0.10f, 0.25f);
        btn.colors = c;
        var rect = obj.GetComponent<RectTransform>();
        rect.anchoredPosition = pos;
        rect.sizeDelta        = new Vector2(320, 65);

        var textObj = new GameObject("Text");
        textObj.transform.SetParent(obj.transform, false);
        var tmp       = textObj.AddComponent<TMPro.TextMeshProUGUI>();
        tmp.text      = label;
        tmp.fontSize  = 24;
        tmp.color     = Color.white;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        var tr        = textObj.GetComponent<RectTransform>();
        tr.anchorMin  = Vector2.zero;
        tr.anchorMax  = Vector2.one;
        tr.offsetMin  = tr.offsetMax = Vector2.zero;
        return obj;
    }

    static Sprite CreateCircleSprite()
    {
        int size = 128;
        var tex  = new Texture2D(size, size, TextureFormat.RGBA32, false);
        var px   = new Color[size * size];
        var c    = new Vector2(size / 2f, size / 2f);
        float r  = size / 2f - 2;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                px[y * size + x] = Vector2.Distance(new Vector2(x, y), c) <= r ? Color.white : Color.clear;
        tex.SetPixels(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // Layout helpers
    static void StretchFull(GameObject obj)
    {
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = r.offsetMax = Vector2.zero;
    }
    static void SetRect(GameObject obj, float x, float y, float w, float h)
    {
        var r = obj.GetComponent<RectTransform>();
        r.anchoredPosition = new Vector2(x, y);
        r.sizeDelta        = new Vector2(w, h);
    }
    static void AnchorTopLeft(GameObject obj, float x, float y, float w, float h)
    {
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0, 1); r.anchorMax = new Vector2(0, 1);
        r.pivot     = new Vector2(0, 1);
        r.anchoredPosition = new Vector2(x, y); r.sizeDelta = new Vector2(w, h);
    }
    static void AnchorTopCenter(GameObject obj, float x, float y, float w, float h)
    {
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.5f, 1); r.anchorMax = new Vector2(0.5f, 1);
        r.pivot     = new Vector2(0.5f, 1);
        r.anchoredPosition = new Vector2(x, y); r.sizeDelta = new Vector2(w, h);
    }
    static void AnchorBottom(GameObject obj, float x, float y, float w, float h)
    {
        var r = obj.GetComponent<RectTransform>();
        r.anchorMin = new Vector2(0.5f, 0); r.anchorMax = new Vector2(0.5f, 0);
        r.pivot     = new Vector2(0.5f, 0);
        r.anchoredPosition = new Vector2(x, y); r.sizeDelta = new Vector2(w, h);
    }
}

public static class SPExt
{
    public static void SetObjRef(this SerializedProperty prop, Object val)
    {
        if (prop != null) prop.objectReferenceValue = val;
    }
}
