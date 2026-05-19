using UnityEngine;
using UnityEngine.InputSystem;

public class CharacterSpawner : MonoBehaviour
{
    public Transform p1SpawnPoint;
    public Transform aiSpawnPoint;

    private void Start()
    {
        if (SelectionManager.Instance == null) return;

        CharacterStats p1Stats = SelectionManager.Instance.characterDb.GetCharacter(SelectionManager.Instance.p1SelectedIndex);
        GameObject p1 = Instantiate(p1Stats.characterPrefab, p1SpawnPoint.position, Quaternion.identity);

        p1.GetComponent<CharacterAI>().enabled = false;

        CharacterStats aiStats = SelectionManager.Instance.characterDb.GetCharacter(SelectionManager.Instance.aiSelectedIndex);
        GameObject ai = Instantiate(aiStats.characterPrefab, aiSpawnPoint.position, Quaternion.identity);

        ai.GetComponent<PlayerInput>().enabled = false;
        ai.GetComponent<CharacterAI>().enabled = true;
    }
}
