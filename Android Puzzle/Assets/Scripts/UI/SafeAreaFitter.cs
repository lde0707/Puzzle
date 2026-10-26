using UnityEngine;

namespace Puzzle.UI
{
    /// <summary>Attach to a direct child of a full-screen Screen Space Overlay Canvas.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rectTransform;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void OnEnable()
        {
            rectTransform = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            if (lastSafeArea != Screen.safeArea ||
                lastScreenSize.x != Screen.width || lastScreenSize.y != Screen.height)
                Apply();
        }

        private void Apply()
        {
            if (Screen.width <= 0 || Screen.height <= 0)
                return;

            lastSafeArea = Screen.safeArea;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);
            rectTransform.anchorMin = new Vector2(
                lastSafeArea.xMin / Screen.width, lastSafeArea.yMin / Screen.height);
            rectTransform.anchorMax = new Vector2(
                lastSafeArea.xMax / Screen.width, lastSafeArea.yMax / Screen.height);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
