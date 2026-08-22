using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    public int currentEnergy;
    private PlayerController Controller;

    private void Awake()
    {
        Controller = GetComponent<PlayerController>();
    }

    private void Start()
    {
        currentEnergy = Controller.stats.startingEnergy;
    }
    public void AddEnergy(int amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, Controller.stats.maxEnergy);
        UpdateUI();
    }

    public bool TrySpendEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            UpdateUI();
            return true;
        }
        return false;
    }

    public void ResetEnergy()
    {
        currentEnergy = Controller.stats.startingEnergy;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (Controller != null)
            UIEvents.OnEnergyChanged?.Invoke(Controller.PlayerIndex, currentEnergy);
    }
}
