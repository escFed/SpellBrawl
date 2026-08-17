// Cursor personalizado
using UnityEngine;
using UnityEngine.UI;
public class CustomCursor : MonoBehaviour
{
    [SerializeField] private Image clicker;

    void Start()
    {
        Cursor.visible = false; // ocultar cursor del sistema
    }

    void Update()
    {
        if (clicker != null)
        {
            // En Canvas Overlay funciona directo
            clicker.rectTransform.position = Input.mousePosition;
        }
    }
}
