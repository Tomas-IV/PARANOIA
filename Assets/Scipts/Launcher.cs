    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using System.Linq;
using System;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;
    //public Action OnRoom;


    [SerializeField] TMP_InputField roomNameInputField;
    [SerializeField] TMP_InputField nicknameInputField;
    [SerializeField] TMP_Text errorText;
    [SerializeField] TMP_Text roomNameText;
    [SerializeField] Transform roomListContent;
    [SerializeField] GameObject roomListItemPrefab;
    [SerializeField] Transform playerListContent;
    [SerializeField] GameObject PlayerListItemPrefab;
    [SerializeField] GameObject startGameButton;

    void Awake()
    {
        if(Instance == null)
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
        if (PlayerPrefs.HasKey("Nickname"))
        {
            nicknameInputField.text = PlayerPrefs.GetString("Nickname");
        }

        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby()
    {
        MenuManager.Instance.OpenMenu("title");
        Debug.Log("Joined Lobby");
        if (PlayerPrefs.HasKey("Nickname"))
        {
            PhotonNetwork.NickName = PlayerPrefs.GetString("Nickname");
        }
        else
        {
            PhotonNetwork.NickName = "Player " + UnityEngine.Random.Range(0, 1000).ToString("0000");
        }

        nicknameInputField.text = PhotonNetwork.NickName;
    }

    public void SetNickname()
    {
        string nickname = nicknameInputField.text.Trim();

        if (string.IsNullOrEmpty(nickname))
            return;

        PhotonNetwork.NickName = nickname;

        PlayerPrefs.SetString("Nickname", nickname);
        PlayerPrefs.Save();

        Debug.Log("Nickname guardado: " + nickname);
    }


    public void CreateRoom()
    {
        SetNickname();

        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }

        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnJoinedRoom()
    {
        MenuManager.Instance.OpenMenu("room");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;


        for (int i = 0; i < players.Count(); i++)
        {
            Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        errorText.text = "Room Creation Failed: " + message;
        Debug.LogError("Room Creation Failed: " + message);
        MenuManager.Instance.OpenMenu("error");
    }

    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.Instance.OpenMenu("loading");
    }

    public void QuitGame()
    {
        StartCoroutine(QuitRoutine());
    }

    private IEnumerator QuitRoutine()
    {
        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();

        while (PhotonNetwork.InRoom)
            yield return null;

        if (PhotonNetwork.IsConnected)
            PhotonNetwork.Disconnect();

        while (PhotonNetwork.IsConnected)
            yield return null;

        Application.Quit();
    }

    public void JoinRoom(RoomInfo info)
    {
        SetNickname();

        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.Instance.OpenMenu("loading");
    }

    public override void OnLeftRoom()
    {
        MenuManager.Instance.OpenMenu("title");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("[NET] Desconectado: " + cause);

        errorText.text = GetErrorMessage(cause);
        MenuManager.Instance.OpenMenu("error");

        StartCoroutine(Reconnect());
    }
    IEnumerator Reconnect()
    {
        yield return new WaitForSeconds(3f);

        Debug.Log("[NET] Reconnecting...");

        PhotonNetwork.ConnectUsingSettings();
    }

    string GetErrorMessage(DisconnectCause cause)
    {
        switch (cause)
        {
            case DisconnectCause.ServerTimeout:
                return "Se perdió la conexión con el servidor.";

            case DisconnectCause.ClientTimeout:
                return "Tu conexión es inestable.";

            case DisconnectCause.ExceptionOnConnect:
                return "No se pudo conectar.";

            case DisconnectCause.MaxCcuReached:
                return "Servidor lleno.";

            case DisconnectCause.InvalidAuthentication:
                return "Error de autenticación.";

            default:
                return "Error de red.";
        }
    }

    //public void SetNickname()
    //{
    //    string nickname = nicknameInputField.text.Trim();

    //    if (string.IsNullOrEmpty(nickname))
    //        return;

    //    PhotonNetwork.NickName = nickname;

    //    PlayerPrefs.SetString("Nickname", nickname);
    //    PlayerPrefs.Save();

    //    Debug.Log("Nickname guardado: " + nickname);
    //}

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }

        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(PlayerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newPlayer);
    }
}

