#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SixteenBit.Editor.QA
{
    public class EditorQAWindow : EditorWindow
    {
        private const string SettingsAssetPath = "Assets/Editor/QA/EditorQASettings.asset";
        private const string SettingsParentPath = "Assets/Editor";
        private const string SettingsFolderPath = "Assets/Editor/QA";
        private Vector2 _scroll;
        private EditorQASettings _settings;

        [MenuItem("Epoch Breaker/QA/Open QA Checklist", priority = 30)]
        public static void Open()
        {
            var window = GetWindow<EditorQAWindow>("QA Checklist");
            window.minSize = new Vector2(720, 500);
            window.Show();
        }

        private void OnEnable()
        {
            _settings = LoadOrCreateSettings();
        }

        private void OnGUI()
        {
            if (_settings == null)
            {
                _settings = LoadOrCreateSettings();
                if (_settings == null) return;
            }

            EditorGUI.BeginChangeCheck();

            DrawHeader();
            DrawMeta();
            DrawControls();
            DrawChecklist();
            DrawFooter();

            if (EditorGUI.EndChangeCheck())
                EditorUtility.SetDirty(_settings);
        }

        private void DrawHeader()
        {
            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Epoch Breaker - In-Editor QA Checklist", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this checklist to validate readability, visual recognition, and playability in the editor. Results are saved to a ScriptableObject asset.", MessageType.Info);
            EditorGUILayout.LabelField(QAValidationRunner.GetLastRunSummary(), EditorStyles.miniLabel);
        }

        private void DrawMeta()
        {
            EditorGUILayout.BeginVertical("box");
            _settings.TesterName = EditorGUILayout.TextField("Tester", _settings.TesterName);
            _settings.BuildOrScene = EditorGUILayout.TextField("Scene/Build", _settings.BuildOrScene);
            _settings.DeviceOrResolution = EditorGUILayout.TextField("Device/Resolution", _settings.DeviceOrResolution);
            _settings.Notes = EditorGUILayout.TextArea(_settings.Notes, GUILayout.MinHeight(48));
            EditorGUILayout.LabelField("Last Updated (UTC)", _settings.LastUpdatedUtc);
            EditorGUILayout.EndVertical();
        }

        private void DrawControls()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Run Automated Validation", GUILayout.Height(28)))
            {
                QAValidationRunner.RunAll();
            }
            if (GUILayout.Button("Mark All Passed", GUILayout.Height(28)))
            {
                _settings.MarkAllPassed();
                MarkDirty();
            }
            if (GUILayout.Button("Reset Checklist", GUILayout.Height(28)))
            {
                _settings.ResetAll();
                MarkDirty();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawChecklist()
        {
            EditorGUILayout.Space(6);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            string currentSection = null;
            foreach (var item in _settings.Items)
            {
                if (currentSection != item.Section)
                {
                    currentSection = item.Section;
                    EditorGUILayout.Space(6);
                    EditorGUILayout.LabelField(currentSection, EditorStyles.boldLabel);
                }

                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                item.Passed = EditorGUILayout.Toggle(item.Passed, GUILayout.Width(24));
                EditorGUILayout.LabelField(item.Title, EditorStyles.boldLabel);
                EditorGUILayout.EndHorizontal();

                if (!string.IsNullOrWhiteSpace(item.Description))
                {
                    EditorGUILayout.LabelField(item.Description, EditorStyles.wordWrappedLabel);
                }

                item.Notes = EditorGUILayout.TextArea(item.Notes, GUILayout.MinHeight(40));
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawFooter()
        {
            EditorGUILayout.Space(4);
            if (GUILayout.Button("Save", GUILayout.Height(24)))
            {
                _settings.TouchTimestamp();
                MarkDirty();
            }
        }

        private static EditorQASettings LoadOrCreateSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<EditorQASettings>(SettingsAssetPath);
            if (settings != null) return settings;

            if (!AssetDatabase.IsValidFolder(SettingsParentPath))
                AssetDatabase.CreateFolder("Assets", "Editor");
            if (!AssetDatabase.IsValidFolder(SettingsFolderPath))
                AssetDatabase.CreateFolder(SettingsParentPath, "QA");

            settings = CreateInstance<EditorQASettings>();
            settings.Items = BuildDefaultChecklist();
            settings.TouchTimestamp();
            AssetDatabase.CreateAsset(settings, SettingsAssetPath);
            AssetDatabase.SaveAssets();
            return settings;
        }

        private static List<QAItem> BuildDefaultChecklist()
        {
            return new List<QAItem>
            {
                new QAItem
                {
                    Section = "Boot & Title",
                    Title = "Title screen readable",
                    Description = "Game title and menu items are legible at intended target resolution.",
                    Required = true
                },
                new QAItem
                {
                    Section = "Boot & Title",
                    Title = "Primary actions recognizable",
                    Description = "Start, Continue, and Options are visually distinct and easy to locate.",
                    Required = true
                },
                new QAItem
                {
                    Section = "HUD & UI",
                    Title = "HUD readable during gameplay",
                    Description = "Health, ammo, era, and objective indicators remain readable in motion.",
                    Required = true
                },
                new QAItem
                {
                    Section = "HUD & UI",
                    Title = "Touch controls visible",
                    Description = "On-screen controls are readable and do not overlap critical gameplay visuals.",
                    Required = true
                },
                new QAItem
                {
                    Section = "Gameplay",
                    Title = "Player and enemies recognizable",
                    Description = "Player sprite, enemy silhouettes, and projectiles are distinct and easy to identify.",
                    Required = true
                },
                new QAItem
                {
                    Section = "Gameplay",
                    Title = "Level geometry readable",
                    Description = "Destructible vs solid tiles are visually distinguishable at a glance.",
                    Required = true
                },
                new QAItem
                {
                    Section = "Gameplay",
                    Title = "Game is playable",
                    Description = "Player can move, shoot, take damage, and complete objective without blockers.",
                    Required = true
                },
                new QAItem
                {
                    Section = "Gameplay",
                    Title = "Camera framing stable",
                    Description = "Camera keeps the player centered and avoids excessive jitter.",
                    Required = true
                },
                new QAItem
                {
                    Section = "Pause & Menus",
                    Title = "Pause menu readable",
                    Description = "Pause menu text is readable and buttons are accessible.",
                    Required = true
                },
                new QAItem
                {
                    Section = "Completion",
                    Title = "Level complete UI readable",
                    Description = "Completion messaging and rewards are readable and not obstructed.",
                    Required = true
                },
                new QAItem
                {
                    Section = "Completion",
                    Title = "Return to title works",
                    Description = "Flow returns to title or next level without errors.",
                    Required = true
                }
            };
        }

        private void MarkDirty()
        {
            EditorUtility.SetDirty(_settings);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
