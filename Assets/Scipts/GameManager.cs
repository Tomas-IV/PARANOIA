using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;

    public enum GameState
    {
        WaitingPlayers,
        Playing,
        EndGame
    }

    [SerializeField] private int minPlayers = 2;

    public GameState CurrentState { get; private set; }

    public List<PlayerManager> Players { get; } = new();

    private bool gameStarted;
    private int winnerTeam = -1;

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

    public override void OnJoinedRoom()
    {
        TryStartMatch();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        TryStartMatch();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CheckWinCondition();
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            CheckWinCondition();
        }
    }

    private void TryStartMatch()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (PhotonNetwork.CurrentRoom == null)
            return;

        if (gameStarted)
            return;

        if (PhotonNetwork.CurrentRoom.PlayerCount < minPlayers)
            return;

        AssignTeams();

        gameStarted = true;

        photonView.RPC(
            nameof(RPC_SetGameState),
            RpcTarget.AllBuffered,
            GameState.Playing);

        Debug.Log("[GameManager] Match Started");
    }

    [PunRPC]
    private void RPC_SetGameState(GameState state)
    {
        CurrentState = state;
    }

    private void AssignTeams()
    {
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < players.Length; i++)
        {
            Hashtable props = new Hashtable();

            props["team"] = i % 2;

            players[i].SetCustomProperties(props);
        }

        Debug.Log("[GameManager] Teams Assigned");
    }

    public int GetPlayerTeam(Player player)
    {
        if (player.CustomProperties.TryGetValue("team", out object team))
            return (int)team;

        return -1;
    }

    public void CheckWinCondition()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        bool team0Alive = false;
        bool team1Alive = false;

        foreach (PlayerManager player in Players)
        {
            if (player == null)
                continue;

            if (player.LifeState != PlayerManager.PlayerLifeState.Alive)
                continue;

            if (player.Team == 0)
                team0Alive = true;

            if (player.Team == 1)
                team1Alive = true;
        }

        if (team0Alive && team1Alive)
            return;

        winnerTeam = team0Alive ? 0 : 1;

        EndGame();
    }

    private void EndGame()
    {
        photonView.RPC(
            nameof(RPC_EndGame),
            RpcTarget.All,
            winnerTeam);
    }

    [PunRPC]
    private void RPC_EndGame(int winningTeam)
    {
        winnerTeam = winningTeam;

        CurrentState = GameState.EndGame;

        Debug.Log($"TEAM {winnerTeam} WINS");

        StartCoroutine(ReturnToLobby());
    }

    private IEnumerator ReturnToLobby()
    {
        yield return new WaitForSeconds(5f);

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();

            while (PhotonNetwork.InRoom)
                yield return null;
        }

        PhotonNetwork.LoadLevel(0);
    }
}
