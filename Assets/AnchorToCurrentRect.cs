using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class AnchorToCurrentRect : MonoBehaviour
{
    [ContextMenu("Bake Scale And Set Anchors")]
    public void BakeScaleAndSetAnchors()
    {
        RectTransform rt = GetComponent<RectTransform>();
        RectTransform parent = rt.parent as RectTransform;

        if (parent == null)
        {
            Debug.LogWarning("Parent must be a RectTransform.");
            return;
        }

        // Get current world corners
        Vector3[] worldCorners = new Vector3[4];
        rt.GetWorldCorners(worldCorners);

        Vector3[] parentCorners = new Vector3[4];
        parent.GetWorldCorners(parentCorners);

        Vector3 parentBottomLeft = parentCorners[0];
        Vector3 parentTopRight = parentCorners[2];

        float parentWidth = parentTopRight.x - parentBottomLeft.x;
        float parentHeight = parentTopRight.y - parentBottomLeft.y;

        float left = (worldCorners[0].x - parentBottomLeft.x) / parentWidth;
        float right = (worldCorners[3].x - parentBottomLeft.x) / parentWidth;

        float bottom = (worldCorners[0].y - parentBottomLeft.y) / parentHeight;
        float top = (worldCorners[1].y - parentBottomLeft.y) / parentHeight;

        // Match anchors to occupied space
        rt.anchorMin = new Vector2(left, bottom);
        rt.anchorMax = new Vector2(right, top);

        // Remove offsets
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        // Reset scale
        rt.localScale = Vector3.one;
    }

    private void Reset()
    {
        BakeScaleAndSetAnchors();
    }
}