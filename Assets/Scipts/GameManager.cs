using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public enum GameState
    {
        WaitingPlayers,
        Playing,
        Victory,
        Defeat
    }

    [Header("Configuración del Juego")]
    [SerializeField] private int minPlayers = 2;
    [SerializeField] private int waitingTime = 10;

    [Header("Pantallas / Fondos")]
    [SerializeField] private GameObject fondoVictoria;
    [SerializeField] private GameObject fondoDerrota;

    [Header("UI Espera")]
    [SerializeField] private GameObject waitingCanvas;
    [SerializeField] private TMP_Text waitingText;

    public GameState CurrentState { get; private set; }
    public List<PlayerManager> Players { get; } = new();

    private bool gameStarted;
    private int jugadoresMuertos = 0; // Contador de jugadores caídos

    private Coroutine waitingCoroutine;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
             Destroy(gameObject);
    }

    private void Start()
    {
        Time.timeScale = 1f;
        CurrentState = GameState.WaitingPlayers;
        TryStartMatch();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TryStartMatch();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            LeaveGame();
        }
    }

    public override void OnJoinedRoom()
    {
        CurrentState = GameState.WaitingPlayers;
        TryStartMatch();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // Si alguien se va, podríamos chequear condiciones de victoria/derrota aquí en el futuro
    }

    private void PauseGame()
    {
        photonView.RPC(nameof(RPC_PauseGame),RpcTarget.All);
    }

    private void ResumeGame()
    {
        photonView.RPC(nameof(RPC_ResumeGame),RpcTarget.All);
    }

    private void TryStartMatch()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (PhotonNetwork.CurrentRoom == null)
            return;

        if (gameStarted)
            return;

        if (PhotonNetwork.CurrentRoom.PlayerCount >= minPlayers)
        {
            StartGame();
        }
        else
        {
            if (waitingCoroutine == null)
            {
                waitingCoroutine = StartCoroutine(PlayerWaitingCountdown());
            }
        }

        Debug.Log("[GameManager] Match Started");
    }

    private IEnumerator PlayerWaitingCountdown()
    {
        PauseGame();
        int timer = waitingTime;


        while (timer > 0)
        {
            photonView.RPC(nameof(RPC_UpdateWaitingText),RpcTarget.All,timer);


            yield return new WaitForSecondsRealtime(1f);


            // Entró otro jugador
            if (PhotonNetwork.CurrentRoom.PlayerCount >= minPlayers)
            {
                StartGame();
                yield break;
            }


            timer--;
        }


        // No llegaron jugadores suficientes
        photonView.RPC(nameof(RPC_NoPlayersFound),RpcTarget.All);


        yield return new WaitForSecondsRealtime(3f);

        ResumeGame();

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();

            while (PhotonNetwork.InRoom)
                yield return null;
        }


        PhotonNetwork.LoadLevel(0);
    }

    private void StartGame()
    {
        ResumeGame();
        if (waitingCoroutine != null)
        {
            StopCoroutine(waitingCoroutine);
            waitingCoroutine = null;
        }


        gameStarted = true;


        photonView.RPC(nameof(RPC_SetGameState),RpcTarget.AllBuffered,GameState.Playing);


        photonView.RPC(nameof(RPC_HideWaitingUI),RpcTarget.All);


        Debug.Log("[GameManager] Match Started");
    }


    [PunRPC]
    private void RPC_SetGameState(GameState state)
    {
        CurrentState = state;
    }

    [PunRPC]
    private void RPC_UpdateWaitingText(int seconds)
    {
        //if (waitingCanvas != null)
        //    waitingCanvas.SetActive(true);


        if (waitingText != null)
        {
            waitingText.text =
                "Esperando otro jugador...\n\n" +
                "Comenzando en: " + seconds;
        }
    }

    [PunRPC]
    private void RPC_NoPlayersFound()
    {
        if (waitingText != null)
        {
            waitingText.text =
                "No se encontraron suficientes jugadores.\n\n" +
                "Volviendo al menú...";
        }
    }

    [PunRPC]
    private void RPC_HideWaitingUI()
    {
        if (waitingCanvas != null)
            waitingText.gameObject.SetActive(false);
    }

    [PunRPC]
    private void RPC_PauseGame()
    {
        Time.timeScale = 0f;
    }

    [PunRPC]
    private void RPC_ResumeGame()
    {
        Time.timeScale = 1f;
    }


    // --- LÓGICA DE VICTORIA ---
    // Llamar a esta función desde el script del Boss cuando muere
    public void BossDerrotado()
    {
        SaveManager.Instancia.SumarVictoria();

        // Solo enviamos la victoria si estábamos jugando
        if (CurrentState == GameState.Playing)
        {
            photonView.RPC(nameof(RPC_Victory), RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_Victory()
    {
        CurrentState = GameState.Victory;
        Debug.Log("VICTORY");

        // Encendemos el fondo de victoria
        if (fondoVictoria != null)
            fondoVictoria.SetActive(true);

        StartCoroutine(ReturnToLobby());
    }

    // --- LÓGICA DE DERROTA ---
    // Llamar a esta función desde el script del jugador cuando muere
    public void JugadorMuerto()
    {
        if (CurrentState == GameState.Playing)
        {
            photonView.RPC(nameof(RPC_RegistrarMuerte), RpcTarget.All);
        }
    }

    [PunRPC]
    private void RPC_RegistrarMuerte()
    {
        jugadoresMuertos++;
        Debug.Log($"Jugador muerto. Total de muertos: {jugadoresMuertos}");

        // Si el MasterClient detecta que murieron todos los de la sala, manda la orden de derrota
        if (PhotonNetwork.IsMasterClient && jugadoresMuertos >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Defeat();
        }
    }

    public void Defeat()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        SaveManager.Instancia.SumarDerrota();
        photonView.RPC(nameof(RPC_Defeat), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_Defeat()
    {
        CurrentState = GameState.Defeat;
        Debug.Log("DEFEAT");

        // Encendemos el fondo de derrota
        if (fondoDerrota != null)
            fondoDerrota.SetActive(true);

        StartCoroutine(ReturnToLobby());
    }

    // --- MANEJO DE SALA ---
    private IEnumerator ReturnToLobby()
    {
        // Espera 5 segundos mostrando la pantalla de victoria/derrota
        yield return new WaitForSeconds(5f);

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();

            while (PhotonNetwork.InRoom)
                yield return null;
        }

        PhotonNetwork.LoadLevel(0);
    }

    private void LeaveGame()
    {
        if (!PhotonNetwork.InRoom)
        {
            SceneManager.LoadScene(0);
            return;
        }

        StartCoroutine(LeaveRoomRoutine());
    }

    private IEnumerator LeaveRoomRoutine()
    {
        PhotonNetwork.LeaveRoom();

        while (PhotonNetwork.InRoom)
            yield return null;

        PhotonNetwork.LoadLevel(0);
    }

}