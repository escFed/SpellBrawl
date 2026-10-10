using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MatchTutorialView : MonoBehaviour
{
    [Serializable]
    public class Page
    {
        public string name;
        public GameObject content;
        public Button nextButton;
        public bool showControls;
        [Header("Optional HUD highlight")]
        public RectTransform highlightFrame;
        public bool followP1Stocks;
        public Vector2 stockPadding = new Vector2(30f, 24f);
        [Header("Optional card preview, in display order")]
        public GameObject[] previewCards;
    }

    [Header("Canvas")]
    [SerializeField] private Image backdrop;
    [SerializeField] private Page[] pages;
    [SerializeField] private Toggle hideFutureTutorialToggle;

    private UIManager hud;
    private Action<bool> onFinished;
    private int currentPage;
    private int lastAdvanceFrame = -1;
    private bool isOpen;
    private int visiblePreviewCards;
    private ControlsTutorial controls;
    private readonly Vector3[] targetCorners = new Vector3[4];

    public bool IsConfigured
    {
        get
        {
            if (pages == null || pages.Length == 0 || backdrop == null || hideFutureTutorialToggle == null)
                return false;

            foreach (Page page in pages)
            {
                if (page == null || page.content == null || page.nextButton == null)
                    return false;
            }

            return true;
        }
    }

    public void Open(UIManager uiManager, Action<bool> finished)
    {
        if (!IsConfigured)
        {
            Debug.LogError("[MatchTutorial] Assign the backdrop, pages, buttons and toggle in the Canvas.", this);
            return;
        }

        hud = uiManager;
        controls = GetComponentInChildren<ControlsTutorial>(true);
        if (controls == null)
            controls = hud != null ? hud.controlsTutorial : null;
        onFinished = finished;
        currentPage = 0;
        visiblePreviewCards = 0;
        lastAdvanceFrame = Time.frameCount;
        isOpen = true;
        hideFutureTutorialToggle.isOn = false;

        foreach (Page page in pages)
            page.nextButton.onClick.AddListener(Advance);

        ShowPage();
    }

    private void Update()
    {
        if (!isOpen)
            return;

        bool keyboardAdvance = Keyboard.current != null &&
            (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
        bool gamepadAdvance = Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame;
        if (keyboardAdvance || gamepadAdvance)
            Advance();
    }

    private void LateUpdate()
    {
        if (isOpen && pages[currentPage].followP1Stocks)
            PositionStockFrame(pages[currentPage]);
    }

    public void Advance()
    {
        if (!isOpen || Time.frameCount == lastAdvanceFrame)
            return;
        lastAdvanceFrame = Time.frameCount;

        Page current = pages[currentPage];
        if (current.previewCards != null && current.previewCards.Length >= 4 && visiblePreviewCards == 2)
        {
            visiblePreviewCards = 4;
            UpdateCardPreview(current);
            return;
        }

        if (currentPage == pages.Length - 1)
        {
            bool hideFutureTutorials = hideFutureTutorialToggle.isOn;
            Action<bool> finished = onFinished;
            onFinished = null;
            finished?.Invoke(hideFutureTutorials);
            return;
        }

        currentPage++;
        visiblePreviewCards = 0;
        ShowPage();
    }

    public void Close()
    {
        if (!isOpen)
            return;

        isOpen = false;
        controls?.HideForTutorial();
        HideAllCardPreviews();
        RemoveButtonListeners();
        gameObject.SetActive(false);
    }

    private void ShowPage()
    {
        for (int i = 0; i < pages.Length; i++)
            pages[i].content.SetActive(i == currentPage);

        Page page = pages[currentPage];
        bool showControls = page.showControls;
        backdrop.enabled = !showControls && !page.followP1Stocks;
        if (showControls)
        {
            if (controls != null && controls.transform.parent == transform)
                controls.transform.SetSiblingIndex(1);
            controls?.ShowForTutorial();
        }
        else
            controls?.HideForTutorial();

        HideAllCardPreviews();
        if (page.previewCards != null && page.previewCards.Length >= 4)
        {
            visiblePreviewCards = 2;
            UpdateCardPreview(page);
        }

        if (page.followP1Stocks)
            PositionStockFrame(page);

        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(page.nextButton.gameObject);
    }

    private void HideAllCardPreviews()
    {
        foreach (Page page in pages)
        {
            if (page?.previewCards == null)
                continue;
            foreach (GameObject card in page.previewCards)
                if (card != null) card.SetActive(false);
        }
    }

    private void UpdateCardPreview(Page page)
    {
        for (int i = 0; i < page.previewCards.Length; i++)
            if (page.previewCards[i] != null)
                page.previewCards[i].SetActive(i < visiblePreviewCards);
    }

    private void PositionStockFrame(Page page)
    {
        if (hud == null || page.highlightFrame == null || hud.p1_life == null)
            return;

        RectTransform parent = page.highlightFrame.parent as RectTransform;
        if (parent == null)
            return;

        Vector2 min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
        Vector2 max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
        bool found = false;
        foreach (GameObject stock in hud.p1_life)
        {
            if (stock == null || !stock.activeInHierarchy || !(stock.transform is RectTransform rect))
                continue;
            rect.GetWorldCorners(targetCorners);
            foreach (Vector3 corner in targetCorners)
            {
                Vector3 local = parent.InverseTransformPoint(corner);
                min = Vector2.Min(min, local);
                max = Vector2.Max(max, local);
                found = true;
            }
        }

        if (!found)
            return;

        page.highlightFrame.anchorMin = page.highlightFrame.anchorMax = new Vector2(0.5f, 0.5f);
        page.highlightFrame.pivot = new Vector2(0.5f, 0.5f);
        page.highlightFrame.anchoredPosition = (min + max) * 0.5f;
        page.highlightFrame.sizeDelta = max - min + page.stockPadding;
    }

    private void RemoveButtonListeners()
    {
        if (pages == null)
            return;

        foreach (Page page in pages)
        {
            if (page?.nextButton != null)
                page.nextButton.onClick.RemoveListener(Advance);
        }
    }

    private void OnDestroy()
    {
        RemoveButtonListeners();
    }
}
