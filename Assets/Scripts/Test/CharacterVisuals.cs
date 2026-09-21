using UnityEngine;

[DisallowMultipleComponent]
public class CharacterVisuals : MonoBehaviour
{
    private static int OutlineColorId = Shader.PropertyToID("_OutlineColor");

    private SpriteRenderer targetRenderer;
    private MaterialPropertyBlock propertyBlock;

    private void Start()
    {
        CharacterCoordinator character = GetComponent<CharacterCoordinator>();
        if (character == null)
            return;

        Initialize(character.Sprite, character.PlayerIndex);

        if (character.stats != null && character.stats.characterIcon != null)
            UIEvents.OnIconSet?.Invoke(character.PlayerIndex, character.stats.characterIcon);
    }

    public void Initialize(SpriteRenderer spriteRenderer, int playerIndex)
    {
        targetRenderer = spriteRenderer;

        if (targetRenderer == null)
        {
            Debug.LogWarning($"[CharacterVisuals] No SpriteRenderer found on '{name}'.", this);
            return;
        }

        Material material = targetRenderer.sharedMaterial;

        if (material == null || !material.HasProperty(OutlineColorId))
        {
            Debug.LogWarning($"[CharacterVisuals] Material on '{name}' does not contain _OutlineColor.", this);
            return;
        }

        ApplyPlayerColor(playerIndex);
    }

    public void ApplyPlayerColor(int playerIndex)
    {
        if (targetRenderer == null)
            return;

        propertyBlock ??= new MaterialPropertyBlock();

        targetRenderer.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor(OutlineColorId, PlayerColors.Get(playerIndex));
        targetRenderer.SetPropertyBlock(propertyBlock);
    }
}
