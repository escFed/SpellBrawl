using UnityEngine;
using GameAnalyticsSDK;

public class AnalyticsManager : MonoBehaviour, IGameAnalyticsATTListener
{
    public static AnalyticsManager Instance { get; private set; }

    private const string ConsentKey = "PrivacyConsentGranted";

    private void Awake()
    {
        // Configuración de Singleton para persistencia
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        CheckAndInitialize();
    }

    // --- SECCIÓN 1: FLUJO DE CONSENTIMIENTO Y PRIVACIDAD ---

    public void CheckAndInitialize()
    {
        // Si el usuario ya dio consentimiento en el pasado, inicializamos según la plataforma
        if (HasUserConsented())
        {
            ProcessPlatformInitialization();
        }
        else
        {
            // AQUÍ: Si es la primera vez, disparen su interfaz de UI/Pop-up de privacidad.
            // No se llama a GameAnalytics.Initialize() hasta que elijan "Aceptar".
            Debug.Log("[Analytics] Esperando consentimiento del jugador...");
        }
    }

    private bool HasUserConsented()
    {
        // Retorna verdadero si ya aceptó previamente (guardado local)
        return PlayerPrefs.GetInt(ConsentKey, 0) == 1;
    }

    public void SetUserConsent(bool accepted)
    {
        PlayerPrefs.SetInt(ConsentKey, accepted ? 1 : 0);
        PlayerPrefs.Save();

        if (accepted)
        {
            ProcessPlatformInitialization();
        }
        else
        {
            // Opcional: GameAnalytics permite setear flag de restricción si el usuario deniega RGPD
            GameAnalytics.SetEnabledManualLogHandling(false);
            Debug.Log("[Analytics] El usuario rechazó el seguimiento.");
        }
    }

    private void ProcessPlatformInitialization()
    {
        // Manejo nativo de Apple ATT para iOS 14.5+
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            GameAnalytics.RequestTrackingAuthorization(this);
        }
        else
        {
            // Android / PC / WebGL inicializan directo si ya hay consentimiento de privacidad
            GameAnalytics.Initialize();
            Debug.Log("[Analytics] SDK Inicializado correctamente.");
        }
    }

    // Listeners obligatorios de iOS (ATT)
    public void GameAnalyticsATTListenerNotDetermined() { GameAnalytics.Initialize(); }
    public void GameAnalyticsATTListenerRestricted() { GameAnalytics.Initialize(); }
    public void GameAnalyticsATTListenerDenied() { GameAnalytics.Initialize(); }
    public void GameAnalyticsATTListenerAuthorized() { GameAnalytics.Initialize(); }


    // --- SECCIÓN 2: ENVÍO DE MÉTRICAS DE SPELL BRAWL ---

    // 1. Balance de IA y Combate (Llamar al terminar la partida)
    public void LogMatchResult(string nivelIA, string personajeJugador, bool victoria, int vidasRestantes)
    {
        if (!HasUserConsented()) return;

        string resultado = victoria ? "Victoria" : "Derrota";
        // Ejemplo: "IA:ia_hard:wizard:Victoria" -> Valor: Vidas restantes
        string eventPath = $"IA:{nivelIA}:{personajeJugador}:{resultado}";

        GameAnalytics.NewDesignEvent(eventPath, vidasRestantes);
    }

    // 2. Balance de Cartas - Uso individual (Llamar al activar la carta)
    public void LogCardUsed(string personajeJugador, string idCarta)
    {
        if (!HasUserConsented()) return;

        // Ejemplo: "Cartas:Uso:warrior:fireball" -> Valor: 1 (Para contar ocurrencias/Pick rate)
        string eventPath = $"Cartas:Uso:{personajeJugador}:{idCarta}";

        GameAnalytics.NewDesignEvent(eventPath, 1f);
    }

    // 3. Balance de Cartas - Tiempo de primer uso (Llamar una sola vez por partida)
    public void LogFirstCardTime(string personajeJugador, float segundosTranscurridos)
    {
        if (!HasUserConsented()) return;

        string eventPath = $"Cartas:PrimerUso:{personajeJugador}";
        GameAnalytics.NewDesignEvent(eventPath, segundosTranscurridos);
    }

    // 4. Métricas de Físicas - Average KnockBack Received (Llamar al recibir impactos fuertes)
    public void LogKnockbackReceived(string personajeJugador, float fuerzaKnockback)
    {
        if (!HasUserConsented()) return;

        string eventPath = $"Fisicas:KnockbackRecibido:{personajeJugador}";
        GameAnalytics.NewDesignEvent(eventPath, fuerzaKnockback);
    }
}