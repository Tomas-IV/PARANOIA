using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerSpawner : MonoBehaviourPunCallbacks
{
    [SerializeField] private GameObject[] playerPrefabs;

    [SerializeField] private Transform[] spawnPositions;

    bool spawned;

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

        GameObject prefab = GetPlayerPrefab();

        PhotonNetwork.Instantiate(prefab.name,spawn.position,Quaternion.identity);
    }

    private GameObject GetPlayerPrefab()
    {
        int avatar = (int)PlayerInfo.GetAvatar(PhotonNetwork.LocalPlayer);

        if (avatar < 0 || avatar >= playerPrefabs.Length)
        {
            return playerPrefabs[0];
        }

        return playerPrefabs[avatar];
    }
}
