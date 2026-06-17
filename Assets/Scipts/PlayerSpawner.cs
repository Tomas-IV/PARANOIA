using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPositions;
    //[SerializeField] private List<Transform> spawnPositions = new List<Transform>();

    bool spawned = false;

    void Start()
    {
        TrySpawn();
    }

    public override void OnJoinedRoom()
    {
        TrySpawn();
    }

    void TrySpawn()
    {
        if (!PhotonNetwork.InRoom) return;
        if (spawned) return;

        spawned = true;

        int index = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        Transform spawn = spawnPositions[index % spawnPositions.Length];

        PhotonNetwork.Instantiate(playerPrefab.name,spawn.position,Quaternion.identity);
    }
}
