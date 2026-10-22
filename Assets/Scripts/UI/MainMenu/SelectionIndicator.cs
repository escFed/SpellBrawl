using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(Image))]
public class SelectionIndicator : MonoBehaviour
{
    private static readonly Color NeutralColor = new Color32(255, 220, 0, 255);

    [Header("Movement")]
    [SerializeField] private Vector2 offset;
    [SerializeField] private float smoothTime = 0.06f;

    [SerializeField] private bool matchSelectionSize;
    [SerializeField] private Vector2 sizePadding;

    [Header("Owner Labels")]
    [SerializeField] private TextMeshProUGUI p1Label;
    [SerializeField] private TextMeshProUGUI p2Label;
    [SerializeField, FormerlySerializedAs("aiLabel")] private TextMeshProUGUI aiP2Label;
    [SerializeField] private TextMeshProUGUI aiP1Label;

    private Vector3[] worldCorners = new Vector3[4];
    private RectTransform indicatorTransform;
    private Image indicatorImage;
    private Vector2 movementVelocity;
    private Selectable colorSource;
    private CharacterSelectUI characterSelection;
    private DeckBuilderUI deckSelection;

    private void Awake()
    {
        indicatorTransform = (RectTransform)transform;
        indicatorImage = GetComponent<Image>();
        indicatorImage.raycastTarget = false;
        indicatorImage.color = NeutralColor;
    }

    private void OnEnable()
    {
        indicatorImage.enabled = false;
        SetLabelsVisible(false, false, false, false);
        indicatorImage.color = NeutralColor;
        movementVelocity = Vector2.zero;
        colorSource = null;
        characterSelection = null;
        deckSelection = null;
    }

    private void LateUpdate()
    {
        Selectable selectedOption = GetCurrentSelection();
        RectTransform selection = selectedOption != null
            ? UIFocus.GetSelectionFrameTarget(selectedOption)
            : null;
        RectTransform indicatorParent = indicatorTransform.parent as RectTransform;

        RefreshPresentation(selectedOption);

        if (selection == null || indicatorParent == null)
        {
            indicatorImage.enabled = false;
            SetLabelsVisible(false, false, false, false);
            return;
        }

        selection.GetWorldCorners(worldCorners);
        Vector3 worldCenter = (worldCorners[0] + worldCorners[2]) * 0.5f;
        Vector3 localCenter = indicatorParent.InverseTransformPoint(worldCenter);
        Vector2 targetPosition = new Vector2(localCenter.x, localCenter.y) + offset;

        indicatorImage.enabled = true;

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

        
    }

    private Selectable GetCurrentSelection()
    {
        if (EventSystem.current == null)
            return null;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;
        if (selectedObject == null || selectedObject == gameObject || !selectedObject.activeInHierarchy)
            return null;

        Selectable selectable = selectedObject.GetComponent<Selectable>();

        if (!UIFocus.IsMenuOption(selectable) || !selectable.IsActive() || !selectable.IsInteractable())
            return null;

        return selectable;
    }

    private void RefreshPresentation(Selectable selectedOption)
    {
        if (selectedOption != colorSource)
        {
            colorSource = selectedOption;
            characterSelection = selectedOption != null
                ? selectedOption.GetComponentInParent<CharacterSelectUI>()
                : null;
            deckSelection = selectedOption != null
                ? selectedOption.GetComponentInParent<DeckBuilderUI>()
                : null;
        }

        Color targetColor = NeutralColor;
        bool hasOwner = characterSelection != null || deckSelection != null;
        bool showP1 = false;
        bool showP2 = false;
        bool showAIP2 = false;
        bool showAIP1 = false;
        if (hasOwner)
        {
            PlayerSlot slot = characterSelection != null
                ? characterSelection.SelectingSlot
                : deckSelection.SelectingSlot;

            SelectionManager selection = SelectionManager.Instance;
            bool isAI = selection != null && !selection.isTrainingMode &&
                selection.GetControlMode(slot) == PlayerMode.AI;
            showAIP1 = isAI && slot == PlayerSlot.PlayerOne;
            showAIP2 = isAI && slot == PlayerSlot.PlayerTwo;
            showP1 = !isAI && slot == PlayerSlot.PlayerOne;
            showP2 = !isAI && slot == PlayerSlot.PlayerTwo;

            TextMeshProUGUI activeLabel = showAIP1 ? aiP1Label
                : showAIP2 ? aiP2Label
                : showP1 ? p1Label
                : p2Label;
            targetColor = activeLabel != null ? activeLabel.color : TeamColors.Get(slot);
        }

        if (indicatorImage.color != targetColor)
            indicatorImage.color = targetColor;
        SetLabelsVisible(showP1, showP2, showAIP2, showAIP1);
    }

    private void SetLabelsVisible(bool showP1, bool showP2, bool showAIP2, bool showAIP1)
    {
        if (p1Label != null && p1Label.enabled != showP1)
            p1Label.enabled = showP1;
        if (p2Label != null && p2Label.enabled != showP2)
            p2Label.enabled = showP2;
        if (aiP2Label != null && aiP2Label.enabled != showAIP2)
            aiP2Label.enabled = showAIP2;
        if (aiP1Label != null && aiP1Label.enabled != showAIP1)
            aiP1Label.enabled = showAIP1;
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
