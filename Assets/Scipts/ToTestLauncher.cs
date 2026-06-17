using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class ToTestLauncher : MonoBehaviourPunCallbacks
{
    public static ToTestLauncher Instance;
    public Action OnRoom;



    // Start is called before the first frame update
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(Instance);
        }

        PhotonNetwork.ConnectUsingSettings();
    }
    void Start()
    {
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined Lobby");
        PhotonNetwork.JoinRandomOrCreateRoom(roomName: "My Room");
    }
    public override void OnJoinedRoom()
    {
        string roomName = PhotonNetwork.CurrentRoom.Name;
        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount;
        Debug.Log("Joined Room: " + roomName + ", Player Count: " + playerCount);

        OnRoom?.Invoke();
    }
}
