using UnityEngine;

public class DragonBreathVisuals : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private ParticleSystem particleSystem;
    [SerializeField] private CircleCollider2D damageArea;

    [SerializeField] private Sprite breathSprite;
    [SerializeField] private Color breathColor = new Color(1f, 0.5f, 0f, 0.8f); // Naranja fuego
    [SerializeField] private float breathScale = 1.5f;

    private void Awake()
    {
        // Obtener componentes si no están asignados
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (damageArea == null)
            damageArea = GetComponent<CircleCollider2D>();
    }

    private void Start()
    {
        // Configurar la visualización
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = breathSprite;
            spriteRenderer.color = breathColor;
        }

        // Configurar escala
        transform.localScale = Vector3.one * breathScale;

        // Reproducir partículas si existen
        if (particleSystem != null)
            particleSystem.Play();
    }

    public void SetVisuals(Sprite sprite, Color color, float scale = 1.5f)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = sprite;
            spriteRenderer.color = color;
        }

        transform.localScale = Vector3.one * scale;
    }

    public void Destroy()
    {
        Destroy(gameObject);
    }
}
