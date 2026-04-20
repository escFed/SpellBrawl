using UnityEngine;
using UnityEngine.UI;

public class EnergyManager : MonoBehaviour
{
    [Header("Energy Settings")]
    public int maxEnergy = 100;
    public int startingEnergy = 50;
    public int currentEnergy;

    private Slider energySlider;

    private void Awake()
    {
        currentEnergy = startingEnergy;
    }

    public void SetUIElements(Slider slider)
    {
        energySlider = slider;

        if (energySlider != null)
        {
            energySlider.minValue = 0;
            energySlider.maxValue = maxEnergy;
            energySlider.value = currentEnergy;
        }
        UpdateUI();
    }
    public void AddEnergy(int amount)
    {
        currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
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
        currentEnergy = startingEnergy;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (energySlider != null) energySlider.value = currentEnergy;
    }
}
