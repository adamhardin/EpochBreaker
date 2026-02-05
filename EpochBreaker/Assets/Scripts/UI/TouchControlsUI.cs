using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace EpochBreaker.UI
{
    /// <summary>
    /// On-screen touch controls for mobile: D-pad (left/right), Jump button, and Attack button.
    /// Only visible on touch-capable devices. Also creates the EventSystem if missing.
    /// </summary>
    public class TouchControlsUI : MonoBehaviour
    {
        private Canvas _canvas;

        private void Start()
        {
            // Only show on touch devices (or always in editor for testing)
            #if UNITY_EDITOR
            CreateUI();
            #elif UNITY_IOS || UNITY_ANDROID
            CreateUI();
            #endif
        }

        private void CreateUI()
        {
            var canvasGO = new GameObject("TouchCanvas");
            canvasGO.transform.SetParent(transform);
            _canvas = canvasGO.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvas.sortingOrder = 95;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();

            // Left arrow (bottom-left)
            CreateDirectionButton(canvasGO.transform, "Left", new Vector2(100, 120),
                "<", -1f);

            // Right arrow (bottom-left, right of left arrow)
            CreateDirectionButton(canvasGO.transform, "Right", new Vector2(240, 120),
                ">", 1f);

            // Jump button (bottom-right)
            CreateActionButton(canvasGO.transform, "Jump", new Vector2(-120, 140),
                "A", true);

            // Attack/target cycle button (bottom-right, above jump)
            CreateActionButton(canvasGO.transform, "Attack", new Vector2(-220, 280),
                "B", false);
        }

        private void CreateDirectionButton(Transform parent, string name, Vector2 position,
            string label, float moveValue)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.25f);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 0);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(100, 100);
            rect.anchoredPosition = position;

            // Label
            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            var text = textGO.AddComponent<Text>();
            text.text = label;
            text.fontSize = 40;
            text.color = new Color(1f, 1f, 1f, 0.6f);
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            // Event triggers for press/release
            var trigger = go.AddComponent<EventTrigger>();

            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((_) => { Gameplay.InputManager.MoveX = moveValue; });
            trigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener((_) => {
                // Only reset if we were providing this direction
                if (Mathf.Sign(Gameplay.InputManager.MoveX) == Mathf.Sign(moveValue))
                    Gameplay.InputManager.MoveX = 0f;
            });
            trigger.triggers.Add(pointerUp);
        }

        private void CreateActionButton(Transform parent, string name, Vector2 position,
            string label, bool isJump)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 0.25f);

            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1, 0);
            rect.anchorMax = new Vector2(1, 0);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(110, 110);
            rect.anchoredPosition = position;

            // Label
            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            var text = textGO.AddComponent<Text>();
            text.text = label;
            text.fontSize = 36;
            text.color = new Color(1f, 1f, 1f, 0.6f);
            text.alignment = TextAnchor.MiddleCenter;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;

            var trigger = go.AddComponent<EventTrigger>();

            var pointerDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener((_) => {
                if (isJump)
                {
                    Gameplay.InputManager.JumpPressed = true;
                    Gameplay.InputManager.JumpHeld = true;
                }
                else
                {
                    Gameplay.InputManager.AttackPressed = true;
                }
            });
            trigger.triggers.Add(pointerDown);

            var pointerUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener((_) => {
                if (isJump)
                    Gameplay.InputManager.JumpHeld = false;
            });
            trigger.triggers.Add(pointerUp);
        }
    }
}
