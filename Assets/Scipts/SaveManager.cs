using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

// --- ESTRUCTURA DE DATOS ---
[System.Serializable]
public class PlayerData
{
    // Datos nuevos que pediste
    public string ultimoNickname = "JugadorNuevo";
    public int avatarSeleccionado = 0; // Usamos un int para guardar el ID o índice del avatar
    public int partidasGanadas = 0;
    public int partidasPerdidas = 0;

    // Datos anteriores (los dejo por si aún los usas en el modo zombies)
    public int maxWavesSurvived = 0;
    public int totalZombiesKilled = 0;
}

// --- MANEJADOR PRINCIPAL ---
public class SaveManager : MonoBehaviourPunCallbacks
{
    public static SaveManager Instancia; // Creado como Singleton para acceder fácil desde otros scripts

    public PlayerData datosDelJuego = new PlayerData();
    private string rutaArchivo;

    private void Awake()
    {
        // Configuramos el Singleton
        if (Instancia == null)
        {
            Instancia = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        rutaArchivo = Path.Combine(Application.persistentDataPath, "progreso_zombies.json");

        // Cargamos los datos al iniciar
        CargarDatos();

        // Comenté la creación de UI automática por ahora para que no te estorbe en la partida, 
        // ya que el guardado ahora es interno con la tecla R.
        // CrearInterfazVisual(); 
    }

    private void Update()
    {
        // Guardado al presionar la tecla R durante la partida
        if (Input.GetKeyDown(KeyCode.R))
        {
            GuardarDatosEnPartida();
        }
    }

    // --- LÓGICA DE GUARDADO ---
    public void GuardarDatosEnPartida()
    {
        // 1. RECOPILAR DATOS: 
        // Aquí es donde el SaveManager le pide la info a tus otros scripts antes de guardar.
        // NOTA: Tendrás que adaptar estas líneas dependiendo de cómo se llamen las variables en tus otros scripts.

        if (PhotonNetwork.IsConnected && PhotonNetwork.LocalPlayer != null)
        {
            datosDelJuego.ultimoNickname = PhotonNetwork.LocalPlayer.NickName;
        }

        /* 
        EJEMPLOS DE CÓMO CONECTARLO CON TUS OTROS SCRIPTS (Descomenta y adapta según tu código):
        
        // Para el Avatar (asumiendo que tu script se llama AvatarSelection)
        // datosDelJuego.avatarSeleccionado = AvatarSelection.instancia.obtenerAvatarActual();

        // Para las partidas ganadas/perdidas (asumiendo que GameManager lleva la cuenta)
        // datosDelJuego.partidasGanadas = GameManager.instancia.victoriasTotales;
        // datosDelJuego.partidasPerdidas = GameManager.instancia.derrotasTotales;
        */

        // 2. GUARDAR EN EL ARCHIVO JSON
        string textoJson = JsonUtility.ToJson(datosDelJuego, true);
        File.WriteAllText(rutaArchivo, textoJson);

        Debug.Log("¡Progreso guardado con éxito presionando R!");
        Debug.Log($"Guardado: Nick {datosDelJuego.ultimoNickname} | Avatar ID {datosDelJuego.avatarSeleccionado} | G/P {datosDelJuego.partidasGanadas}/{datosDelJuego.partidasPerdidas}");
    }

    public void CargarDatos()
    {
        if (File.Exists(rutaArchivo))
        {
            datosDelJuego = JsonUtility.FromJson<PlayerData>(File.ReadAllText(rutaArchivo));
            Debug.Log("Datos cargados correctamente.");
        }
        else
        {
            Debug.Log("No hay archivo de guardado previo, se creará uno nuevo al guardar.");
        }
    }

    // --- MÉTODOS PARA MODIFICAR DATOS DESDE OTROS SCRIPTS ---
    // Puedes llamar a estos métodos desde GameManager cuando termine una partida
    public void SumarVictoria()
    {
        datosDelJuego.partidasGanadas++;
        GuardarDatosEnPartida();
    }

    public void SumarDerrota()
    {
        datosDelJuego.partidasPerdidas++;
        GuardarDatosEnPartida();
    }

    public void EstablecerAvatar(int idAvatar)
    {
        datosDelJuego.avatarSeleccionado = idAvatar;
        // No guardamos automáticamente aquí para esperar a que el jugador presione R o termine la partida
    }
}