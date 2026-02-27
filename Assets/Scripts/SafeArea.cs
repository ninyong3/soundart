using UnityEngine;

public class SafeArea : MonoBehaviour
{
    private RectTransform panel;
    private void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position+safeArea.size;
        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;
        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
    }
    private void Awake()
    {
        panel = GetComponent<RectTransform>();
        ApplySafeArea();
    }
}
