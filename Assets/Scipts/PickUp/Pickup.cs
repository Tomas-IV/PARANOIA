using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public abstract class Pickup : MonoBehaviourPun
{
    private bool picked;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (picked)
            return;

        if (!collision.gameObject.CompareTag("Player"))
            return;

        PhotonView playerView = collision.gameObject.GetComponent<PhotonView>();

        if (playerView == null)
            playerView = collision.gameObject.GetComponentInParent<PhotonView>();

        if (playerView == null || !playerView.IsMine)
            return;

        photonView.RPC(nameof(RPC_RequestPickup),RpcTarget.MasterClient,playerView.ViewID);
    }

    [PunRPC]
    public void RPC_RequestPickup(int playerViewID)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (picked)
            return;

        PhotonView playerView = PhotonView.Find(playerViewID);

        if (playerView == null)
            return;

        picked = true;

        Apply(playerView.gameObject);

        gameObject.SetActive(false);
    }

    protected abstract void Apply(GameObject player);

}