using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.SceneManagement;

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

    [Header("Pantallas / Fondos")]
    [SerializeField] private GameObject fondoVictoria;
    [SerializeField] private GameObject fondoDerrota;

    public GameState CurrentState { get; private set; }
    public List<PlayerManager> Players { get; } = new();

    private bool gameStarted;
    private int jugadoresMuertos = 0;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
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
        TryStartMatch();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
    }

    private void TryStartMatch()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (PhotonNetwork.CurrentRoom == null) return;
        if (gameStarted) return;
        if (PhotonNetwork.CurrentRoom.PlayerCount < minPlayers) return;

        gameStarted = true;
        photonView.RPC(nameof(RPC_SetGameState), RpcTarget.AllBuffered, GameState.Playing);

        Debug.Log("[GameManager] Match Started");
    }

    [PunRPC]
    private void RPC_SetGameState(GameState state)
    {
        CurrentState = state;
    }

    // --- LÓGICA DE VICTORIA ---
    public void BossDerrotado()
    {
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

        // --- CONEXIÓN CON SAVEMANAGER: Guardamos la victoria automáticamente ---
        if (SaveManager.Instancia != null)
        {
            SaveManager.Instancia.SumarVictoria();
        }

        if (fondoVictoria != null)
            fondoVictoria.SetActive(true);

        StartCoroutine(ReturnToLobby());
    }

    // --- LÓGICA DE DERROTA ---
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

        if (PhotonNetwork.IsMasterClient && jugadoresMuertos >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            Defeat();
        }
    }

    public void Defeat()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC(nameof(RPC_Defeat), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_Defeat()
    {
        CurrentState = GameState.Defeat;
        Debug.Log("DEFEAT");

        // --- CONEXIÓN CON SAVEMANAGER: Guardamos la derrota automáticamente ---
        if (SaveManager.Instancia != null)
        {
            SaveManager.Instancia.SumarDerrota();
        }

        if (fondoDerrota != null)
            fondoDerrota.SetActive(true);

        StartCoroutine(ReturnToLobby());
    }

    // --- MANEJO DE SALA ---
    private IEnumerator ReturnToLobby()
    {
        yield return new WaitForSeconds(5f);

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            while (PhotonNetwork.InRoom) yield return null;
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
        while (PhotonNetwork.InRoom) yield return null;
        PhotonNetwork.LoadLevel(0);
    }
}