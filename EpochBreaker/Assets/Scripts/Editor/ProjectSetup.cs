#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace SixteenBit.Editor
{
    /// <summary>
    /// Editor tool to set up the Epoch Breaker project.
    /// Creates the required scene, configures tags and layers,
    /// and sets up project settings.
    ///
    /// Menu: Epoch Breaker > Setup Project
    /// </summary>
    public static class ProjectSetup
    {
        [MenuItem("Epoch Breaker/Setup Project", priority = 1)]
        public static void SetupProject()
        {
            SetupTags();
            SetupLayers();
            SetupPhysics2D();
            CreateBootstrapScene();

            Debug.Log("[Epoch Breaker] Project setup complete!");
            Debug.Log("[Epoch Breaker] Open 'Assets/Scenes/Bootstrap.unity' and press Play.");
        }

        [MenuItem("Epoch Breaker/Create Bootstrap Scene", priority = 2)]
        public static void CreateBootstrapScene()
        {
            // Ensure Scenes directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            // Create a new empty scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Note: EventSystem is created at runtime by GameManager

            // Add a placeholder camera (GameManager will replace it, but
            // Unity needs one in the scene for the editor view)
            var camGO = new GameObject("EditorCamera");
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 9f;
            cam.backgroundColor = new Color(0.08f, 0.06f, 0.15f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            camGO.transform.position = new Vector3(0, 0, -10);

            // Note: GameManager auto-creates itself via [RuntimeInitializeOnLoadMethod]
            // so we don't need to add it to the scene manually.

            // Save scene
            string scenePath = "Assets/Scenes/Bootstrap.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[Epoch Breaker] Created scene: {scenePath}");

            // Set as the scene in Build Settings
            var buildScenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(scenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes;
            Debug.Log("[Epoch Breaker] Set Bootstrap scene in Build Settings.");
        }

        [MenuItem("Epoch Breaker/Generate Test Level (Play Mode)", priority = 10)]
        public static void GenerateTestLevel()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("[Epoch Breaker] Enter Play Mode first, then use this menu item.");
                return;
            }

            var gm = Gameplay.GameManager.Instance;
            if (gm != null)
            {
                gm.StartGame();
            }
        }

        private static void SetupTags()
        {
            // Unity tags "Player" and "MainCamera" are built-in.
            // Add "Enemy" tag if it doesn't exist.
            SerializedObject tagManager = new SerializedObject(
                AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));

            SerializedProperty tags = tagManager.FindProperty("tags");

            bool hasEnemy = false;
            for (int i = 0; i < tags.arraySize; i++)
            {
                if (tags.GetArrayElementAtIndex(i).stringValue == "Enemy")
                {
                    hasEnemy = true;
                    break;
                }
            }

            if (!hasEnemy)
            {
                tags.InsertArrayElementAtIndex(tags.arraySize);
                tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = "Enemy";
                tagManager.ApplyModifiedProperties();
                Debug.Log("[Epoch Breaker] Added 'Enemy' tag.");
            }
        }

        private static void SetupLayers()
        {
            // For now we use Default layer for everything.
            // Could add dedicated layers later for player, enemies, projectiles.
            Debug.Log("[Epoch Breaker] Using default layers. Custom layers can be added later.");
        }

        private static void SetupPhysics2D()
        {
            // Configure 2D physics for platformer gameplay
            Physics2D.gravity = new Vector2(0, -30f);
            Debug.Log("[Epoch Breaker] Set Physics2D gravity to (0, -30).");
        }
    }
}
#endif
