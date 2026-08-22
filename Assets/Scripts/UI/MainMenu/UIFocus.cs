using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class UIFocus
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void HideMouseCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.None;
    }

    public static IEnumerator SelectFirstNextFrame(GameObject root)
    {
        yield return null;
        SelectFirst(root);
    }

    public static void SelectFirst(GameObject root)
    {
        if (root == null || !root.activeInHierarchy || EventSystem.current == null)
            return;

        Selectable[] selectables = root.GetComponentsInChildren<Selectable>(false);

        foreach (Selectable selectable in selectables)
        {
            if (IsMenuOption(selectable) && selectable.IsActive() && selectable.IsInteractable())
            {
                EventSystem.current.SetSelectedGameObject(selectable.gameObject);
                return;
            }
        }

        Clear();
    }

    public static void Clear()
    {
        if (EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(null);
    }

    public static bool IsMenuOption(Selectable selectable)
    {
        return selectable is Button || selectable is UICard || selectable is Slider;
    }

    public static RectTransform GetSelectionFrameTarget(Selectable selectable)
    {
        if (!IsMenuOption(selectable))
            return null;

        if (selectable is Slider slider && slider.handleRect != null)
            return slider.handleRect;

        return selectable.transform as RectTransform;
    }
}
