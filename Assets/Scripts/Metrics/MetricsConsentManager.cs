using TaloGameServices;
using System.Threading.Tasks;
using UnityEngine;

public class MetricsConsentManager : MonoBehaviour
{

    public static MetricsConsentManager Instance { get; private set; }

    
    public static bool CanSendAMetric() => Instance != null && Instance.GetConsent();
    private const string ConsentKey = "MetricsConsent";

    private void Awake()
    {
     

        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }


    public bool Answered()
    {
        return PlayerPrefs.HasKey(ConsentKey);
    }


    public bool GetConsent()
    {
        return PlayerPrefs.GetInt(ConsentKey, 0) == 1;
    }


    public void Accept()
    {
        PlayerPrefs.SetInt(ConsentKey, 1);
        PlayerPrefs.Save();
    }

    public void Decline()
    {
        PlayerPrefs.SetInt(ConsentKey, 0);
        PlayerPrefs.Save();
    }

    public async Task TrackEventAsync(string eventName)
    {
        if (!GetConsent()) return;

        try
        {
            await Talo.Events.Track(eventName);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"No se pudo enviar el evento '{eventName}': {e.Message}");
        }
    }
}
