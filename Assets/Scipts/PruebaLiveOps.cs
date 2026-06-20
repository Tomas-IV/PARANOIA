//using System.Diagnostics;
//using System.Threading.Tasks;
//using Unity.Services.Authentication;
//using Unity.Services.Core;
//using Unity.Services.RemoteConfig;
//using UnityEngine;
////using System recovery = System; // Excepciones nativo

//public class LiveOpsManager : MonoBehaviour
//{
//    //  entorno de Remote Config sepa a quién pedirle los datos
//    public struct UserAttributes { }
//    public struct AppAttributes { }

//    [Header("Live-Ops Configured Variables")]
//    // 1. Feature Flag: Activa o desactiva una mecánica del juego
//    public bool isSpecialFeatureEnabled = false;

//    // 2. Parámetro Modificable: Por ejemplo, la velocidad de un peligro o precio de un ítem
//    public float globalSpeedMultiplier = 1.0f;

//    [Header("Game References")]
//    [SerializeField] private GameObject specialFeatureObject; // on/off de la feature en el juego

//    async void Awake()
//    {
//        // Es obligatorio inicializar los servicios de Unity 
//        // antes de poder consultar Remote Config.
//        try
//        {
//            await UnityServices.InitializeAsync();

//            if (!AuthenticationService.Instance.IsSignedIn)
//            {
//                await AuthenticationService.Instance.SignInAnonymouslyAsync();
//            }

//            //Debug.Log("Live-Ops: Servicios de Unity autenticados correctamente.");

//            // Una vez logueados, traemos los datos de la nube
//            FetchRemoteConfigValues();
//        }
//        catch (System.Exception e)
//        {
//            //Debug.LogError($"Error inicializando Unity Services: {e.Message}");
//        }
//    }

//    public void FetchRemoteConfigValues()
//    {
//        //Debug.Log("Live-Ops: Solicitando nuevos parámetros a Remote Config...");

//        // Configuramos los atributos (en este caso vacíos para configuraciones globales)
//        var userAttr = new UserAttributes();
//        var appAttr = new AppAttributes();

//        // Vinculamos la respuesta del servidor a nuestro método de callback
//        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;

//        // Hacemos el Fetch  al servidor de Unity
//        RemoteConfigService.Instance.FetchConfigs(userAttr, appAttr);
//    }

//    private void ApplyRemoteSettings(ConfigResponse response)
//    {
//        // Es buena práctica desvincular el evento para evitar llamadas duplicadas a futuro
//        RemoteConfigService.Instance.FetchCompleted -= ApplyRemoteSettings;

//      //  Debug.Log($"Live-Ops: Configuración recibida. Estado de la respuesta: {response.requestOrigin}");

//        // --- ASIGNACIÓN DE PARÁMETROS DESDE LA NUBE ---

//        // Llave 1: Buscamos el booleano para activar/desactivar la feature (Feature Flag)
//        // El segundo parámetro de 'GetBool' es el valor por defecto si no lo encuentra en la web
//        isSpecialFeatureEnabled = RemoteConfigService.Instance.appConfig.GetBool("enable_special_feature", false);

//        // Llave 2: Buscamos el parámetro de modificación numérica
//        globalSpeedMultiplier = RemoteConfigService.Instance.appConfig.GetFloat("global_speed_multiplier", 1.0f);

//        // --- APLICACIÓN DE LA LÓGICA EN EL JUEGO ---
//        ActualizarEstadoDelJuego();
//    }

//    private void ActualizarEstadoDelJuego()
//    {
//        //Debug.Log($"Live-Ops Aplicado -> Feature Activa: {isSpecialFeatureEnabled} | Multiplicador: {globalSpeedMultiplier}");

//        // Activamos o desactivamos físicamente el objeto o componente de la feature en base al flag
//        if (specialFeatureObject != null)
//        {
//            specialFeatureObject.SetActive(isSpecialFeatureEnabled);
//        }

//        // Acá podrías notificar a otros scripts (ej. al Player o al GameManager) 
//        // de que el 'globalSpeedMultiplier' cambió.
//    }
//}

using System.Threading.Tasks;
using Unity.Services.RemoteConfig;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class ExampleSample : MonoBehaviour
{
    public struct userAttributes { }
    public struct appAttributes { }

    async Task InitializeRemoteConfigAsync()
    {
        // initialize handlers for unity game services
        await UnityServices.InitializeAsync();

        // remote config requires authentication for managing environment information
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    async Task Start()
    {
        // initialize Unity's authentication and core services, however check for internet connection
        // in order to fail gracefully without throwing exception if connection does not exist
        if (Utilities.CheckForInternetConnection())
        {
            await InitializeRemoteConfigAsync();
        }

        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
        RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes());
    }

    void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        Debug.Log("RemoteConfigService.Instance.appConfig fetched: " + RemoteConfigService.Instance.appConfig.config.ToString());
    }
}