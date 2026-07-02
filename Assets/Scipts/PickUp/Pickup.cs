using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public abstract class Pickup : MonoBehaviourPun
{
    private bool picked;

    private void OnTriggerEnter(Collider2D other)
    {
        if (picked)
            return;

        if (!other.CompareTag("Player"))
            return;

        PhotonView playerView = other.GetComponent<PhotonView>();

        if (playerView == null || !playerView.IsMine)
            return;

        picked = true;

        Apply(playerView.gameObject);

        photonView.RPC(nameof(RPC_DestroyPickup), RpcTarget.AllBuffered);
    }

    protected abstract void Apply(GameObject player);

    [PunRPC]
    void RPC_DestroyPickup()
    {
        PhotonNetwork.Destroy(gameObject);
    }
}