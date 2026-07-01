using UnityEngine;
using System.IO;
using UnityEngine.UI; // <-- IMPORTANTE: Para poder usar Text e InputField
using TMPro;         // <-- Descomentá esto si usás TextMeshPro en vez de Text tradicional

[System.Serializable]
public class PlayerData
{
    public string lastNickname = "";
    public int lastAvatarIndex = 0;
    public int matchesWon = 0;
    public int matchesLost = 0;
}

public class SaveManager : MonoBehaviour
{
    [Header("Datos en Memoria")]
    public PlayerData datosDelJugador = new PlayerData();

    [Header("Componentes de UI del Menú")]
    public TMP_InputField inputFieldNickname; // Cambiá a 'InputField' si no usás TextMeshPro
    public TMP_Text textoGanadas;             // Cambiá a 'Text' si no usás TextMeshPro
    public TMP_Text textoPerdidas;            // Cambiá a 'Text' si no usás TextMeshPro

    private string rutaArchivo;

    private void Awake()
    {
        rutaArchivo = Path.Combine(Application.persistentDataPath, "historial_pvp.json");
        CargarDatos();

        // APENAS ARRANCA EL JUEGO: Mostramos los datos cargados en la pantalla
        ActualizarInterfazGrafica();
    }

    public void ActualizarInterfazGrafica()
    {
        // 1. Ponemos el último nickname en el cuadro de texto para que no tenga que escribirlo de nuevo
        if (inputFieldNickname != null)
            inputFieldNickname.text = datosDelJugador.lastNickname;

        // 2. Mostramos el historial de victorias y derrotas en los textos del menú
        if (textoGanadas != null)
            textoGanadas.text = "Ganadas: " + datosDelJugador.matchesWon;

        if (textoPerdidas != null)
            textoPerdidas.text = "Perdidas: " + datosDelJugador.matchesLost;
    }

    // Cuando el jugador cambia el texto de su nombre en el menú, guardamos ese cambio
    public void ActualizarNicknameDesdeUI()
    {
        if (inputFieldNickname != null)
        {
            datosDelJugador.lastNickname = inputFieldNickname.text;
            GuardarDatos();
        }
    }

    [ContextMenu("Guardar Datos")]
    public void GuardarDatos()
    {
        try
        {
            string textoJson = JsonUtility.ToJson(datosDelJugador, true);
            File.WriteAllText(rutaArchivo, textoJson);
            Debug.Log("Datos guardados con éxito.");
        }
        catch (System.Exception e) { Debug.LogError("Error al guardar: " + e.Message); }
    }

    [ContextMenu("Cargar Datos")]
    public void CargarDatos()
    {
        if (File.Exists(rutaArchivo))
        {
            try
            {
                string textoJson = File.ReadAllText(rutaArchivo);
                datosDelJugador = JsonUtility.FromJson<PlayerData>(textoJson);
            }
            catch (System.Exception e) { Debug.LogError("Error al cargar: " + e.Message); }
        }
        else
        {
            datosDelJugador = new PlayerData();
        }
    }
}