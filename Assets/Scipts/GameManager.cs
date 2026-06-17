using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] int minPlayers = 1;
    public static GameManager Instance;

    public enum GameState { Waiting, Hiding, Searching, End }
    public GameState currentState;
    public enum ConnectionState { Disconnected, Connecting, Connected, JoiningRoom, InRoom }
    public ConnectionState connectState;

    public float hidingTime = 45f;
    public float searchingTime = 300f;

    private float stateStartTime;
    private float stateDuration;

    private bool gameStarted = false;
    private bool rolesAssigned;
    public int gameResult = -1;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        TryStartGameFlow();
    }

    void Update()
    {
        TryStartGameFlow();
    }

    public override void OnJoinedRoom()
    {
        TryStartGameFlow();
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        TryStartGameFlow();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("[GM] Player joined: " + newPlayer.NickName);

        TryStartGameFlow();

        //  si el juego ya empezó → asignar rol SOLO al nuevo jugador
        if (PhotonNetwork.IsMasterClient && gameStarted && rolesAssigned)
        {
            AssignRoleToNewPlayer(newPlayer);
        }
    }

    // start game flow
    void TryStartGameFlow()
    {
        if (!PhotonNetwork.IsMasterClient || PhotonNetwork.CurrentRoom == null)
            return;

        if (gameStarted)
            return;

        if (PhotonNetwork.CurrentRoom.PlayerCount < minPlayers)
            return;

        Debug.Log("[GM] Starting GameFlow");

        gameStarted = true;

        photonView.RPC("SetGameState", RpcTarget.AllBuffered, GameState.Waiting, 0f, 0f);

        StartCoroutine(GameFlow());
    }

    //main flow
    IEnumerator GameFlow()
    {
        // esperar mínimo jugadores
        while (PhotonNetwork.CurrentRoom.PlayerCount < minPlayers)
            yield return new WaitForSeconds(1f);

        AssignRolesSafe();
        rolesAssigned = true;

        StartState(GameState.Hiding, hidingTime);
        yield return new WaitForSeconds(hidingTime);

        StartState(GameState.Searching, searchingTime);
        yield return new WaitForSeconds(searchingTime);

        EndGame(0);
    }

    //states
    void StartState(GameState newState, float duration)
    {
        stateStartTime = (float)PhotonNetwork.Time;
        stateDuration = duration;

        photonView.RPC(
            "SetGameState",
            RpcTarget.AllBuffered,
            newState,
            stateStartTime,
            stateDuration
        );
    }

    [PunRPC]
    void SetGameState(GameState newState, float startTime, float duration)
    {
        currentState = newState;
        stateStartTime = startTime;
        stateDuration = duration;

        Debug.Log("[GameState] → " + newState);
    }

    public float GetRemainingTime()
    {
        float elapsed = (float)(PhotonNetwork.Time - stateStartTime);
        return Mathf.Max(0, stateDuration - elapsed);
    }

    //role system
    void AssignRolesSafe()
    {
        Player[] players = PhotonNetwork.PlayerList;

        int seekerIndex = Random.Range(0, players.Length);

        foreach (Player p in players)
        {
            Hashtable props = new Hashtable();
            props["role"] = (p == players[seekerIndex]) ? 1 : 0;
            p.SetCustomProperties(props);
        }

        Debug.Log("[Roles] Assigned (safe)");
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("[GM] Disconnected");

        PhotonNetwork.LoadLevel(0);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        CheckGameOver();
    }

    void AssignRoleToNewPlayer(Player newPlayer)
    {
        bool seekerExists = false;

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            if (p.CustomProperties != null &&
                p.CustomProperties.ContainsKey("role") &&
                (int)p.CustomProperties["role"] == 1)
            {
                seekerExists = true;
                break;
            }
        }

        Hashtable props = new Hashtable();
        props["role"] = seekerExists ? 0 : 1;

        newPlayer.SetCustomProperties(props);

        Debug.Log("[Roles] New player → " + (seekerExists ? "Hider" : "Seeker"));
    }

    public void CheckGameOver()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        PlayerController[] players = FindObjectsOfType<PlayerController>();

        bool allHidersCaptured = true;

        foreach (var p in players)
        {
            if (p.Role == 0 && !p.IsCaptured) // hider vivo
            {
                allHidersCaptured = false;
                break;
            }
        }

        if (allHidersCaptured)
        {
            EndGame(1); // seeker win
        }
    }
    private void EndGame(int result)
    {
        photonView.RPC("RPC_EndGame", RpcTarget.All, result);
    }

    [PunRPC]
    public void RPC_EndGame(int result)
    {
        gameResult = result;
        currentState = GameState.End;

        StartCoroutine(WaitAndReturnToMenu(5f));
    }
    IEnumerator WaitAndReturnToMenu(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            yield return new WaitUntil(() => !PhotonNetwork.InRoom);
        }

        PhotonNetwork.LoadLevel(0);
    }
}
