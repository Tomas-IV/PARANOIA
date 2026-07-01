using UnityEngine;
using System.IO;
using TMPro; // Usamos TextMeshPro para los textos del menú

// --- LA ESTRUCTURA DE DATOS QUE SE GUARDA ---
[System.Serializable]
public class PlayerData
{
    public string nicknameJugador1 = "Jugador 1";
    public string nicknameJugador2 = "Jugador 2";
    public int maxWavesSurvived = 0;   // Récord de oleadas sobrevivientes
    public int totalZombiesKilled = 0; // Total acumulado de zombis muertos
}

// --- EL MANEJADOR PRINCIPAL (VA SOLO EN EL MAIN MENU) ---
public class SaveManager : MonoBehaviour
{
    [Header("Datos en Memoria")]
    public PlayerData datosDelJuego = new PlayerData();

    [Header("UI del Main Menu (Asignar aquí)")]
    public TMP_InputField inputP1;
    public TMP_InputField inputP2;
    public TMP_Text textoOleadaRecord;
    public TMP_Text textoZombiesTotales;

    private string rutaArchivo;

    private void Awake()
    {
        // 1. Esto hace que el objeto viaje del Menú al Game sin borrarse
        DontDestroyOnLoad(gameObject);

        // 2. Definimos la ruta del archivo .json en la PC
        rutaArchivo = Path.Combine(Application.persistentDataPath, "progreso_zombies.json");

        // 3. Cargamos los datos guardados y actualizamos la pantalla del menú
        CargarDatos();
        ActualizarInterfazMenu();
    }

    // Muestra los datos del JSON en los textos de tu menú principal
    public void ActualizarInterfazMenu()
    {
        if (inputP1 != null) inputP1.text = datosDelJuego.nicknameJugador1;
        if (inputP2 != null) inputP2.text = datosDelJuego.nicknameJugador2;

        if (textoOleadaRecord != null)
            textoOleadaRecord.text = "Máxima Oleada Lograda: " + datosDelJuego.maxWavesSurvived;

        if (textoZombiesTotales != null)
            textoZombiesTotales.text = "Total Zombis Eliminados: " + datosDelJuego.totalZombiesKilled;
    }

    // --- GUARDAR DATOS (Escribe el archivo JSON en la PC) ---
    public void GuardarDatos()
    {
        // Guardamos los nombres actuales de los inputs antes de escribir el archivo
        if (inputP1 != null) datosDelJuego.nicknameJugador1 = inputP1.text;
        if (inputP2 != null) datosDelJuego.nicknameJugador2 = inputP2.text;

        string textoJson = JsonUtility.ToJson(datosDelJuego, true); //

        // Usamos StreamWriter con bloque 'using' como en el video
        using (StreamWriter writer = new StreamWriter(rutaArchivo))
        {
            writer.Write(textoJson);
        }
        Debug.Log("Progreso guardado en: " + rutaArchivo);
    }

    // --- CARGAR DATOS (Lee el archivo JSON de la PC) ---
    public void CargarDatos()
    {
        if (File.Exists(rutaArchivo))
        {
            string textoJson = "";

            // Usamos StreamReader con bloque 'using' para leer
            using (StreamReader reader = new StreamReader(rutaArchivo))
            {
                textoJson = reader.ReadToEnd(); //
            }

            // Convertimos el texto de vuelta a variables
            datosDelJuego = JsonUtility.FromJson<PlayerData>(textoJson); //
            Debug.Log("Datos cargados correctamente.");
        }
        else
        {
            Debug.Log("No hay archivo previo. Iniciando datos en limpio.");
            datosDelJuego = new PlayerData();
        }
    }
}