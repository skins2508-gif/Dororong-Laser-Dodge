using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public static class BeeSurvivalPrototypeBuilder
{
    private const string ScenePath = "Assets/Scenes/BeeSurvivalPrototype.unity";
    private const string PrefabFolder = "Assets/Prefabs";
    private const string GeneratedFolder = "Assets/Generated";
    private const string AutoBuildKey = "BeeSurvivalPrototypeBuilder.AutoBuilt";

    [InitializeOnLoadMethod]
    private static void AutoBuildOnceWhenEditorCompiles()
    {
        if (Application.isBatchMode) return;
        if (EditorPrefs.GetBool(AutoBuildKey, false)) return;
        if (File.Exists(ScenePath)) return;

        EditorPrefs.SetBool(AutoBuildKey, true);
        EditorApplication.delayCall += BuildBasicPrototype;
    }

    [MenuItem("Tools/Bee Survival/Build Basic Prototype")]
    public static void BuildBasicPrototype()
    {
        EnsureFolders();
        EnsureTags("Player", "Tongue");
        EnsureTmpResources();

        Sprite beeSprite = LoadSpriteByPath("Assets/image/\uAFc0\uBC8C.png");
        Sprite frogSprite = LoadSpriteByPath("Assets/image/\uB3C4\uB85C\uB871-removebg-preview.png");
        Sprite startSprite = LoadSpriteByPath("Assets/image/\uB3C4\uB85C\uB8712.png");
        Sprite honeySprite = LoadSpriteByPath("Assets/image/\uAFc0\uD1B5.png");
        Sprite heartSprite = LoadSpriteByPath("Assets/image/\uD558\uD2B8 \uC774\uBBF8\uC9C0.png");
        Sprite flowerSprite = LoadSpriteByPath("Assets/image/\uAF43 \uC774\uBBF8\uC9C0.png");
        Sprite auraSprite = LoadOrCreateAuraSprite();
        Material tongueMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Tongue_Mat.mat");

        GameObject honeyPrefab = CreateHoneyPrefab(honeySprite);
        GameObject flowerPrefab = CreateFlowerPrefab(flowerSprite, auraSprite);
        GameObject frogPrefab = CreateFrogPrefab(frogSprite, tongueMaterial);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateCamera();
        CreateGlobalLight();
        CreateBee(beeSprite);
        GameObject gameManager = CreateGameManager(frogPrefab, honeyPrefab, flowerPrefab);
        CreateUi(gameManager, heartSprite, startSprite);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings(ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("Bee Survival Prototype", "Prototype scene rebuilt.", "OK");
        }
    }

    private static void EnsureFolders()
    {
        CreateFolderIfMissing("Assets", "Prefabs");
        CreateFolderIfMissing("Assets", "Generated");
        CreateFolderIfMissing("Assets", "Editor");
    }

    private static void EnsureTmpResources() { }

    private static void CreateFolderIfMissing(string parent, string child)
    {
        string path = parent + "/" + child;
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }

    private static void EnsureTags(params string[] tags)
    {
        SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProperty = tagManager.FindProperty("tags");

        foreach (string tag in tags)
        {
            bool exists = false;
            for (int i = 0; i < tagsProperty.arraySize; i++)
            {
                if (tagsProperty.GetArrayElementAtIndex(i).stringValue == tag)
                {
                    exists = true;
                    break;
                }
            }

            if (!exists)
            {
                tagsProperty.InsertArrayElementAtIndex(tagsProperty.arraySize);
                tagsProperty.GetArrayElementAtIndex(tagsProperty.arraySize - 1).stringValue = tag;
            }
        }

        tagManager.ApplyModifiedProperties();
    }

    private static Sprite LoadSpriteByPath(string path)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in allAssets)
            {
                if (asset is Sprite assetSprite)
                {
                    return assetSprite;
                }
            }
        }

        return sprite;
    }

    private static Sprite LoadOrCreateAuraSprite()
    {
        string path = GeneratedFolder + "/Prototype_FlowerAura.png";
        if (!File.Exists(path))
        {
            Texture2D texture = new Texture2D(128, 128, TextureFormat.RGBA32, false);
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(64, 64)) / 64f;
                    float alpha = Mathf.Clamp01(1f - dist) * 0.38f;
                    texture.SetPixel(x, y, new Color(1f, 0.95f, 0.35f, alpha));
                }
            }

            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
        }

        AssetDatabase.ImportAsset(path);
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100f;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static GameObject CreateHoneyPrefab(Sprite honeySprite)
    {
        GameObject root = new GameObject("Honey");
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = honeySprite;
        renderer.sortingOrder = 5;
        root.transform.localScale = Vector3.one * 0.22f;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 2.3f;

        root.AddComponent<HoneyItem>();

        string path = PrefabFolder + "/Honey.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateFlowerPrefab(Sprite flowerSprite, Sprite auraSprite)
    {
        GameObject root = new GameObject("Flower");
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = flowerSprite;
        renderer.sortingOrder = 4;
        root.transform.localScale = Vector3.one * 0.9f;

        CircleCollider2D collider = root.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 1.15f;

        FlowerItem flower = root.AddComponent<FlowerItem>();

        GameObject aura = new GameObject("Aura");
        aura.transform.SetParent(root.transform);
        aura.transform.localPosition = Vector3.zero;
        aura.transform.localScale = Vector3.one * 2.5f;
        SpriteRenderer auraRenderer = aura.AddComponent<SpriteRenderer>();
        auraRenderer.sprite = auraSprite;
        auraRenderer.sortingOrder = 3;

        SetSerializedObject(flower, "auraRenderer", auraRenderer);

        string path = PrefabFolder + "/Flower.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static GameObject CreateFrogPrefab(Sprite frogSprite, Material tongueMaterial)
    {
        GameObject root = new GameObject("DororongEnemy");
        SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
        renderer.sprite = frogSprite;
        renderer.sortingOrder = 6;
        root.transform.localScale = Vector3.one * 0.45f;

        FrogController frog = root.AddComponent<FrogController>();

        GameObject mouth = new GameObject("Mouth");
        mouth.transform.SetParent(root.transform);
        mouth.transform.localPosition = new Vector3(0f, -0.25f, 0f);

        GameObject laser = new GameObject("Laser");
        laser.transform.SetParent(root.transform);
        laser.transform.localPosition = Vector3.zero;
        laser.tag = "Tongue";

        LineRenderer line = laser.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.material = tongueMaterial;
        line.startColor = new Color(1f, 0.08f, 0.16f, 1f);
        line.endColor = new Color(1f, 0.42f, 0.18f, 1f);
        line.sortingOrder = 10;

        EdgeCollider2D edge = laser.AddComponent<EdgeCollider2D>();
        edge.isTrigger = true;

        Rigidbody2D rb = laser.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;

        TongueController tongueController = laser.AddComponent<TongueController>();

        frog.tongue = tongueController;
        frog.mouth = mouth.transform;
        frog.extendSpeed = 12f;
        frog.tongueThickness = 0.2f;

        string path = PrefabFolder + "/Frog.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void CreateCamera()
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.backgroundColor = new Color(0.58f, 0.78f, 0.48f, 1f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CameraBounds2D>();
    }

    private static void CreateGlobalLight()
    {
        System.Type lightType = System.Type.GetType("UnityEngine.Rendering.Universal.Light2D, Unity.RenderPipelines.Universal.Runtime");
        GameObject lightObject = new GameObject("Global Light 2D");
        if (lightType != null)
        {
            lightObject.AddComponent(lightType);
            SerializedObject so = new SerializedObject(lightObject.GetComponent(lightType));
            SerializedProperty lightTypeProperty = so.FindProperty("m_LightType");
            if (lightTypeProperty != null) lightTypeProperty.intValue = 5;
            SerializedProperty intensityProperty = so.FindProperty("m_Intensity");
            if (intensityProperty != null) intensityProperty.floatValue = 1f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void CreateBee(Sprite beeSprite)
    {
        GameObject bee = new GameObject("Bee");
        bee.tag = "Player";
        bee.transform.position = Vector3.zero;
        bee.transform.localScale = Vector3.one * 0.75f;

        SpriteRenderer renderer = bee.AddComponent<SpriteRenderer>();
        renderer.sprite = beeSprite;
        renderer.sortingOrder = 7;

        Rigidbody2D rb = bee.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        CircleCollider2D collider = bee.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.42f;

        bee.AddComponent<BeeController>();
        BeeHealth health = bee.AddComponent<BeeHealth>();
        SetSerializedObject(health, "spriteRenderer", renderer);
    }

    private static GameObject CreateGameManager(GameObject frogPrefab, GameObject honeyPrefab, GameObject flowerPrefab)
    {
        GameObject gameManager = new GameObject("GameManager");
        gameManager.AddComponent<GameFlow>();
        gameManager.AddComponent<GameTimer>();
        gameManager.AddComponent<ScoreManager>();
        FrogWaveManager wave = gameManager.AddComponent<FrogWaveManager>();
        ItemSpawner spawner = gameManager.AddComponent<ItemSpawner>();
        HoneySpawner honeySpawner = gameManager.AddComponent<HoneySpawner>();
        gameManager.AddComponent<RankingManager>();
        NyanCatSwarm nyan = gameManager.AddComponent<NyanCatSwarm>();
        SetSerializedObject(nyan, "nyanVideo", AssetDatabase.LoadAssetAtPath<VideoClip>("Assets/Resources/NyanCat.mp4"));
        SetSerializedObject(nyan, "nyanSound", AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Sound/냥캣.mp3"));

        wave.frogPrefab = frogPrefab;
        wave.enterDuration = 0.35f;
        wave.innerMargin = 1.1f;
        SetSerializedObject(spawner, "honeyPrefab", honeyPrefab);
        SetSerializedObject(spawner, "flowerPrefab", flowerPrefab);
        SetSerializedObject(honeySpawner, "honeyPrefab", honeyPrefab);

        return gameManager;
    }

    private static void CreateUi(GameObject gameManager, Sprite heartSprite, Sprite startSprite)
    {
        GameObject canvasObject = new GameObject("Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();

        Text timerText = CreateText(canvasObject.transform, "TimerText", "00:00:00", 54, TextAnchor.MiddleCenter);
        RectTransform timerRect = timerText.rectTransform;
        timerRect.anchorMin = new Vector2(0.5f, 1f);
        timerRect.anchorMax = new Vector2(0.5f, 1f);
        timerRect.pivot = new Vector2(0.5f, 1f);
        timerRect.anchoredPosition = new Vector2(0f, -36f);
        timerRect.sizeDelta = new Vector2(500f, 90f);
        SetSerializedObject(gameManager.GetComponent<GameTimer>(), "timerText", timerText);

        Text scoreText = CreateText(canvasObject.transform, "ScoreText", "SCORE  000000", 38, TextAnchor.MiddleRight);
        RectTransform scoreRect = scoreText.rectTransform;
        scoreRect.anchorMin = new Vector2(1f, 1f);
        scoreRect.anchorMax = new Vector2(1f, 1f);
        scoreRect.pivot = new Vector2(1f, 1f);
        scoreRect.anchoredPosition = new Vector2(-40f, -42f);
        scoreRect.sizeDelta = new Vector2(420f, 70f);
        SetSerializedObject(gameManager.GetComponent<ScoreManager>(), "scoreText", scoreText);

        GameObject healthObject = new GameObject("HealthUI");
        healthObject.transform.SetParent(canvasObject.transform, false);
        RectTransform healthRect = healthObject.AddComponent<RectTransform>();
        healthRect.anchorMin = new Vector2(0f, 1f);
        healthRect.anchorMax = new Vector2(0f, 1f);
        healthRect.pivot = new Vector2(0f, 1f);
        healthRect.anchoredPosition = new Vector2(40f, -40f);
        healthRect.sizeDelta = new Vector2(260f, 80f);
        HealthUI healthUi = healthObject.AddComponent<HealthUI>();

        Image[] hearts = new Image[3];
        for (int i = 0; i < hearts.Length; i++)
        {
            GameObject heart = new GameObject("Heart" + (i + 1));
            heart.transform.SetParent(healthObject.transform, false);
            Image image = heart.AddComponent<Image>();
            image.sprite = heartSprite;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.anchoredPosition = new Vector2(i * 72f, 0f);
            rect.sizeDelta = new Vector2(64f, 64f);
            hearts[i] = image;
        }
        SetSerializedObject(healthUi, "hpImages", hearts);

        GameObject startPanel = CreateStartPanel(canvasObject.transform, startSprite);
        Button startButton = startPanel.GetComponentInChildren<Button>(true);
        StartScreenUI startUi = canvasObject.AddComponent<StartScreenUI>();
        SetSerializedObject(startUi, "startPanel", startPanel);
        SetSerializedObject(startUi, "startButton", startButton);

        GameObject panel = new GameObject("HighscorePanel");
        panel.transform.SetParent(canvasObject.transform, false);
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.68f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = new Vector2(620f, 660f);

        Text title = CreateText(panel.transform, "TitleText", "GAME OVER", 64, TextAnchor.MiddleCenter);
        title.rectTransform.anchoredPosition = new Vector2(0f, 250f);
        title.rectTransform.sizeDelta = new Vector2(560f, 80f);

        Text finalTime = CreateText(panel.transform, "FinalTimeText", "TIME  00:00:00", 40, TextAnchor.MiddleCenter);
        finalTime.rectTransform.anchoredPosition = new Vector2(0f, 175f);
        finalTime.rectTransform.sizeDelta = new Vector2(560f, 60f);

        Text finalScore = CreateText(panel.transform, "FinalScoreText", "SCORE  000000", 40, TextAnchor.MiddleCenter);
        finalScore.rectTransform.anchoredPosition = new Vector2(0f, 120f);
        finalScore.rectTransform.sizeDelta = new Vector2(560f, 60f);

        Text[] ranks = new Text[5];
        for (int i = 0; i < ranks.Length; i++)
        {
            ranks[i] = CreateText(panel.transform, "Rank" + (i + 1), "Rank #" + (i + 1) + " : 00:00:00", 30, TextAnchor.MiddleCenter);
            ranks[i].rectTransform.anchoredPosition = new Vector2(0f, 45f - i * 44f);
            ranks[i].rectTransform.sizeDelta = new Vector2(560f, 42f);
        }

        GameObject buttonObject = new GameObject("RestartButton");
        buttonObject.transform.SetParent(panel.transform, false);
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(1f, 0.82f, 0.16f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchoredPosition = new Vector2(0f, -255f);
        buttonRect.sizeDelta = new Vector2(260f, 70f);

        Text buttonText = CreateText(buttonObject.transform, "Text", "RESTART", 30, TextAnchor.MiddleCenter);
        buttonText.color = new Color(0.12f, 0.08f, 0.02f, 1f);
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;

        GameOverUI gameOverUi = canvasObject.AddComponent<GameOverUI>();
        UnityEventTools.AddPersistentListener(button.onClick, gameOverUi.RestartGame);
        SetSerializedObject(gameOverUi, "highscorePanel", panel);
        SetSerializedObject(gameOverUi, "finalTimeText", finalTime);
        SetSerializedObject(gameOverUi, "finalScoreText", finalScore);
        SetSerializedObject(gameOverUi, "rankTexts", ranks);
        SetSerializedObject(gameOverUi, "restartButton", button);
    }

    private static GameObject CreateStartPanel(Transform parent, Sprite startSprite)
    {
        GameObject panel = new GameObject("StartPanel");
        panel.transform.SetParent(parent, false);
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.08f, 0.12f, 0.08f, 0.86f);
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image dororong = new GameObject("Dororong2Image").AddComponent<Image>();
        dororong.transform.SetParent(panel.transform, false);
        dororong.sprite = startSprite;
        dororong.preserveAspect = true;
        RectTransform imageRect = dororong.rectTransform;
        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = new Vector2(0f, 110f);
        imageRect.sizeDelta = new Vector2(430f, 430f);

        Text title = CreateText(panel.transform, "StartTitleText", "BEE SURVIVAL", 64, TextAnchor.MiddleCenter);
        title.rectTransform.anchoredPosition = new Vector2(0f, 360f);
        title.rectTransform.sizeDelta = new Vector2(700f, 90f);

        GameObject buttonObject = new GameObject("StartButton");
        buttonObject.transform.SetParent(panel.transform, false);
        Image buttonImage = buttonObject.AddComponent<Image>();
        buttonImage.color = new Color(1f, 0.82f, 0.16f, 1f);
        Button button = buttonObject.AddComponent<Button>();
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.pivot = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -270f);
        buttonRect.sizeDelta = new Vector2(300f, 88f);

        Text buttonText = CreateText(buttonObject.transform, "Text", "START", 36, TextAnchor.MiddleCenter);
        buttonText.color = new Color(0.12f, 0.08f, 0.02f, 1f);
        buttonText.rectTransform.anchorMin = Vector2.zero;
        buttonText.rectTransform.anchorMax = Vector2.one;
        buttonText.rectTransform.offsetMin = Vector2.zero;
        buttonText.rectTransform.offsetMax = Vector2.zero;

        return panel;
    }

    private static Text CreateText(Transform parent, string name, string text, float size, TextAnchor alignment)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);
        Text tmp = textObject.AddComponent<Text>();
        tmp.text = text;
        tmp.fontSize = Mathf.RoundToInt(size);
        tmp.alignment = alignment;
        tmp.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        tmp.color = Color.white;
        tmp.raycastTarget = false;
        RectTransform rect = tmp.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(400f, 80f);
        return tmp;
    }

    private static void SetSerializedObject(Object target, string propertyName, Object value)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null)
        {
            property.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void SetSerializedObject(Object target, string propertyName, Object[] values)
    {
        SerializedObject so = new SerializedObject(target);
        SerializedProperty property = so.FindProperty(propertyName);
        if (property != null && property.isArray)
        {
            property.arraySize = values.Length;
            for (int i = 0; i < values.Length; i++)
            {
                property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();
        }
    }

    private static void AddSceneToBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            if (scene.path == scenePath)
            {
                scene.enabled = true;
                EditorBuildSettings.scenes = scenes.ToArray();
                return;
            }
        }

        scenes.Add(new EditorBuildSettingsScene(scenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}


