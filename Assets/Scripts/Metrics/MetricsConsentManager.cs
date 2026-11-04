using TaloGameServices;
using System;
using System.Threading.Tasks;
using UnityEngine;

public class MetricsConsentManager : MonoBehaviour
{
    public static MetricsConsentManager Instance { get; private set; }

    public static bool CanSendAMetric() => Instance != null && Instance.GetConsent();
    private const string ConsentKey = "MetricsConsent";
    private const string PlayerIdKey = "MetricsPlayerId";

    private Task identifyTask;

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

    // Si ya había aceptado en una sesión anterior, identificar al arrancar.
    private async void Start()
    {
        if (GetConsent())
            await IdentifyAsync();
    }

    public bool Answered() => PlayerPrefs.HasKey(ConsentKey);
    public bool GetConsent() => PlayerPrefs.GetInt(ConsentKey, 0) == 1;

    public void Accept()
    {
        PlayerPrefs.SetInt(ConsentKey, 1);
        PlayerPrefs.Save();
        _ = IdentifyAsync(); // recién ahora se identifica
    }

    public void Decline()
    {
        PlayerPrefs.SetInt(ConsentKey, 0);
        PlayerPrefs.Save();
    }

    // Idempotente: si ya se identificó (o está en curso), reutiliza la misma tarea.
    public Task IdentifyAsync()
    {
        if (identifyTask == null || identifyTask.IsFaulted)
            identifyTask = DoIdentify();
        return identifyTask;
    }

    private async Task DoIdentify()
    {
        try
        {
            string id = PlayerPrefs.GetString(PlayerIdKey, "");
            if (string.IsNullOrEmpty(id))
            {
                id = Guid.NewGuid().ToString();
                PlayerPrefs.SetString(PlayerIdKey, id);
                PlayerPrefs.Save();
            }

            await Talo.Players.Identify("anonymous", id);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"No se pudo identificar al jugador en Talo: {e.Message}");
        }
    }

    public async Task TrackEventAsync(string eventName, params (string key, string value)[] props)
    {
        if (!GetConsent()) return;

        try
        {
            await IdentifyAsync(); // garantiza que haya jugador antes de enviar
            await Talo.Events.Track(eventName, props);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"No se pudo enviar el evento '{eventName}': {e.Message}");
        }
    }
}