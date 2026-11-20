using System.Collections.Generic;
using System.IO;
using BrainrotRunner.CameraLayer;
using BrainrotRunner.Core;
using BrainrotRunner.DebugTools;
using BrainrotRunner.InputLayer;
using BrainrotRunner.Player;
using BrainrotRunner.Progression;
using BrainrotRunner.UI;
using BrainrotRunner.World;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BrainrotRunner.EditorTools
{

    public static class BrainrotSetup
    {
        private const string ConfigFolder = "Assets/brainrotrunner/Config";
        private const string MaterialFolder = "Assets/brainrotrunner/Materials";
        private const string SceneFolder = "Assets/brainrotrunner/Scenes";
        private const string TuningPath = ConfigFolder + "/GameTuning.asset";
        private const string ScenePath = SceneFolder + "/Prototype.unity";
        private const string PalettePath = ConfigFolder + "/TrackPalette.asset";

      
        [MenuItem("Brainrot/Setup/0. Run All Steps")]
        public static void RunAllSteps()
        {
            ApplyAndroidSettings();
            CreateTuningAsset();
            BuildPrototypeScene();
        }


        [MenuItem("Brainrot/Build Android APK (Development)")]
        public static void BuildAndroidApk() => BuildAndroid(true);

   
        [MenuItem("Brainrot/Build Android APK (Release)")]
        public static void BuildAndroidRelease() => BuildAndroid(false);

        private static void BuildAndroid(bool development)
        {
            ApplyAndroidSettings();

            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outputDirectory = Path.Combine(projectRoot, "Builds");
            Directory.CreateDirectory(outputDirectory);

            string outputPath = Path.Combine(outputDirectory,
                development ? "BrainrotRunner.apk" : "BrainrotRunner-release.apk");

            List<string> scenes = new List<string>();
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled)
                    scenes.Add(scene.path);
            }

            if (scenes.Count == 0)
            {
                Debug.LogError("[Brainrot] No scenes in build settings. Run Setup first.");
                return;
            }

            EditorUserBuildSettings.buildAppBundle = false;
            EditorUserBuildSettings.development = development;

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = scenes.ToArray(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = development
                    ? BuildOptions.Development | BuildOptions.ConnectWithProfiler
                    : BuildOptions.None
            };

            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
            UnityEditor.Build.Reporting.BuildSummary summary = report.summary;

            if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[Brainrot] APK built: {outputPath} " +
                          $"({summary.totalSize / 1024f / 1024f:F1} MB, {summary.totalTime.TotalSeconds:F0}s)");
            }
            else
            {
                Debug.LogError("[Brainrot] Build failed: " + summary.result + ", errors: " + summary.totalErrors);
            }
        }

        [MenuItem("Brainrot/Setup/1. Apply Android Settings")]
        public static void ApplyAndroidSettings()
        {
            PlayerSettings.companyName = "Shyian";
            PlayerSettings.productName = "Brainrot Runner";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.shyian.brainrotrunner");

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.startInFullscreen = true;

            EditorUserBuildSettings.selectedBuildTargetGroup = BuildTargetGroup.Android;

            AssetDatabase.SaveAssets();
            Debug.Log("[Brainrot] Android settings applied: portrait, IL2CPP, ARM64, minSdk 24, Linear.");
        }

        [MenuItem("Brainrot/Setup/2. Create Tuning Asset")]
        public static GameTuning CreateTuningAsset()
        {
            EnsureFolder(ConfigFolder);

            GameTuning tuning = AssetDatabase.LoadAssetAtPath<GameTuning>(TuningPath);
            if (tuning != null)
            {
                Debug.Log("[Brainrot] GameTuning already exists, reused.");
                return tuning;
            }

            tuning = ScriptableObject.CreateInstance<GameTuning>();
            AssetDatabase.CreateAsset(tuning, TuningPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Brainrot] GameTuning created at " + TuningPath);
            return AssetDatabase.LoadAssetAtPath<GameTuning>(TuningPath);
        }

        [MenuItem("Brainrot/Setup/3. Build Prototype Scene")]
        public static void BuildPrototypeScene()
        {
            CreateTuningAsset();
            EnsureFolder(MaterialFolder);
            EnsureFolder(SceneFolder);

            EnsureMaterial("M_Road", new Color(0.22f, 0.23f, 0.26f));
            EnsureMaterial("M_Marker", new Color(0.62f, 0.64f, 0.70f));
            EnsureMaterial("M_LaneLine", new Color(0.38f, 0.40f, 0.45f));
            EnsureMaterial("M_Player", new Color(0.95f, 0.75f, 0.15f));
            EnsureMaterial("M_Pillar", new Color(0.30f, 0.45f, 0.55f));
            EnsureMaterial("M_LaneBlock", new Color(0.80f, 0.20f, 0.18f));
            EnsureMaterial("M_JumpBlock", new Color(0.30f, 0.70f, 0.35f));
            EnsureMaterial("M_SlideBar", new Color(0.45f, 0.35f, 0.85f));
            EnsureMaterial("M_Coin", new Color(0.98f, 0.82f, 0.25f));
            EnsurePalette();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();


            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameTuning tuning = AssetDatabase.LoadAssetAtPath<GameTuning>(TuningPath);
            if (tuning == null)
            {
                Debug.LogError("[Brainrot] GameTuning asset not found at " + TuningPath);
                return;
            }

            Material playerMaterial = LoadMaterial("M_Player");

            TrackPalette palette = AssetDatabase.LoadAssetAtPath<TrackPalette>(PalettePath);
            if (palette == null)
            {
                Debug.LogError("[Brainrot] TrackPalette not found at " + PalettePath);
                return;
            }

            palette.road = LoadMaterial("M_Road");
            palette.marker = LoadMaterial("M_Marker");
            palette.laneLine = LoadMaterial("M_LaneLine");
            palette.pillar = LoadMaterial("M_Pillar");
            palette.laneBlock = LoadMaterial("M_LaneBlock");
            palette.jumpBlock = LoadMaterial("M_JumpBlock");
            palette.slideBar = LoadMaterial("M_SlideBar");
            palette.coin = LoadMaterial("M_Coin");
            EditorUtility.SetDirty(palette);
            AssetDatabase.SaveAssets();

            GameObject systems = new GameObject("Systems");
            systems.AddComponent<Bootstrap>();
            GameManager manager = systems.AddComponent<GameManager>();
            SwipeDetector swipe = systems.AddComponent<SwipeDetector>();
            SetObjectField(manager, "tuning", tuning);
            SetObjectField(swipe, "tuning", tuning);

            GameObject lightObject = new GameObject("Directional Light");
            Light lightComponent = lightObject.AddComponent<Light>();
            lightComponent.type = LightType.Directional;
            lightComponent.shadows = LightShadows.Hard;
            lightComponent.intensity = 1.1f;
            lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 0.9f, 0f);
            player.GetComponent<Renderer>().sharedMaterial = playerMaterial;
            PlayerMotor motor = player.AddComponent<PlayerMotor>();
            PlayerController controller = player.AddComponent<PlayerController>();
            player.AddComponent<PlayerCollisionProbe>();
            SetObjectField(motor, "tuning", tuning);
            SetObjectField(controller, "swipeDetector", swipe);

            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.fieldOfView = 60f;
            camera.farClipPlane = 250f;
            camera.backgroundColor = new Color(0.07f, 0.09f, 0.13f);
            FollowCamera followCamera = cameraObject.AddComponent<FollowCamera>();
            SetObjectField(followCamera, "tuning", tuning);
            SetObjectField(followCamera, "target", player.transform);

            systems.AddComponent<GameOverView>();
            systems.AddComponent<CoinSystem>();
            systems.AddComponent<HudView>();

            ScoreSystem scoreSystem = systems.AddComponent<ScoreSystem>();
            SetObjectField(scoreSystem, "tuning", tuning);
            SetObjectField(scoreSystem, "motor", motor);

            TuningOverlay overlay = systems.AddComponent<TuningOverlay>();
            SetObjectField(overlay, "motor", motor);
            SetObjectField(overlay, "swipeDetector", swipe);

            GameObject track = new GameObject("Track");
            ChunkSpawner spawner = track.AddComponent<ChunkSpawner>();
            SetObjectField(spawner, "tuning", tuning);
            SetObjectField(spawner, "palette", palette);
            SetObjectField(spawner, "player", player.transform);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
            AddSceneToBuildSettings(ScenePath);

            Debug.Log("[Brainrot] Prototype scene built at " + ScenePath +
                      ". Play: swipe or arrows / A-D to change lane.");
        }

        private static string MaterialPath(string name) => MaterialFolder + "/" + name + ".mat";

        private static void EnsureMaterial(string name, Color color)
        {
            string path = MaterialPath(name);
            if (AssetDatabase.LoadAssetAtPath<Material>(path) != null)
                return;

            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", 0.1f);
            AssetDatabase.CreateAsset(material, path);
        }

        private static void EnsurePalette()
        {
            if (AssetDatabase.LoadAssetAtPath<TrackPalette>(PalettePath) != null)
                return;

            TrackPalette palette = ScriptableObject.CreateInstance<TrackPalette>();
            AssetDatabase.CreateAsset(palette, PalettePath);
        }

        private static Material LoadMaterial(string name)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath(name));
            if (material == null)
                Debug.LogError("[Brainrot] Material not found: " + MaterialPath(name));

            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            string leaf = Path.GetFileName(path);
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        private static void AddSceneToBuildSettings(string path)
        {
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.path == path)
                    return;
            }

            List<EditorBuildSettingsScene> scenes =
                new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.Insert(0, new EditorBuildSettingsScene(path, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        private static void SetObjectField(Object target, string fieldName, Object value)
        {
            if (value == null)
            {
                Debug.LogError("[Brainrot] Null value for field " + fieldName);
                return;
            }

            SerializedObject serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(fieldName);
            if (property == null)
            {
                Debug.LogWarning("[Brainrot] Serialized field not found: " + fieldName);
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();

            if (property.objectReferenceValue == null)
                Debug.LogError("[Brainrot] Reference did not stick: " + fieldName);

            EditorUtility.SetDirty(target);
        }
    }
}
