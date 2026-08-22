using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class SelectionIndicator : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private Vector2 offset;
    [SerializeField] private float smoothTime = 0.06f;

    [SerializeField] private bool matchSelectionSize;
    [SerializeField] private Vector2 sizePadding;

    private Vector3[] worldCorners = new Vector3[4];
    private RectTransform indicatorTransform;
    private Image indicatorImage;
    private Vector2 movementVelocity;

    private void Awake()
    {
        indicatorTransform = (RectTransform)transform;
        indicatorImage = GetComponent<Image>();
        indicatorImage.raycastTarget = false;
    }

    private void OnEnable()
    {
        indicatorImage.enabled = false;
        movementVelocity = Vector2.zero;
    }

    private void LateUpdate()
    {
        RectTransform selection = GetCurrentSelection();
        RectTransform indicatorParent = indicatorTransform.parent as RectTransform;

        if (selection == null || indicatorParent == null)
        {
            indicatorImage.enabled = false;
            return;
        }

        selection.GetWorldCorners(worldCorners);
        Vector3 worldCenter = (worldCorners[0] + worldCorners[2]) * 0.5f;
        Vector3 localCenter = indicatorParent.InverseTransformPoint(worldCenter);
        Vector2 targetPosition = new Vector2(localCenter.x, localCenter.y) + offset;

        if (!indicatorImage.enabled || smoothTime <= 0f)
        {
            SetLocalPosition(targetPosition);
            movementVelocity = Vector2.zero;
        }
        else
        {
            Vector2 currentPosition = indicatorTransform.localPosition;
            Vector2 nextPosition = Vector2.SmoothDamp(
                currentPosition,
                targetPosition,
                ref movementVelocity,
                smoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime);

            SetLocalPosition(nextPosition);
        }

        if (matchSelectionSize)
            MatchSelectionSize(indicatorParent);

        indicatorImage.enabled = true;
    }

    private RectTransform GetCurrentSelection()
    {
        if (EventSystem.current == null)
            return null;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject == null || selectedObject == gameObject || !selectedObject.activeInHierarchy)
            return null;

        Selectable selectable = selectedObject.GetComponent<Selectable>();

        if (!UIFocus.IsMenuOption(selectable) || !selectable.IsActive() || !selectable.IsInteractable())
            return null;

        return UIFocus.GetSelectionFrameTarget(selectable);
    }

    private void SetLocalPosition(Vector2 position)
    {
        Vector3 currentPosition = indicatorTransform.localPosition;
        indicatorTransform.localPosition = new Vector3(position.x, position.y, currentPosition.z);
    }

    private void MatchSelectionSize(RectTransform indicatorParent)
    {
        Vector3 bottomLeft = indicatorParent.InverseTransformPoint(worldCorners[0]);
        Vector3 topLeft = indicatorParent.InverseTransformPoint(worldCorners[1]);
        Vector3 topRight = indicatorParent.InverseTransformPoint(worldCorners[2]);

        float width = Vector3.Distance(topLeft, topRight) + sizePadding.x;
        float height = Vector3.Distance(bottomLeft, topLeft) + sizePadding.y;

        indicatorTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        indicatorTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }
}
