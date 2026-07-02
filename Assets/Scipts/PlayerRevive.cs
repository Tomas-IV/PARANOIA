using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerRevive : MonoBehaviourPun
{
    [Header("Revive")]
    [SerializeField] private float reviveDistance = 2f;
    [SerializeField] private float reviveTime = 5f;
    [SerializeField] private KeyCode reviveKey = KeyCode.Space;

    private PlayerManager playerManager;
    private PlayerManager reviveTarget;
    private PlayerUI playerUI;
    private PlayerUI reviveTargetUI;


    private float reviveProgress;
    private bool isReviving;

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>();
        playerUI = GetComponentInChildren<PlayerUI>();
    }

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        if (!playerManager.IsAlive())
        {
            CancelRevive();
            return;
        }

        HandleRevive();
    }

    private void HandleRevive()
    {
        PlayerManager target = FindDownedPlayer();

        if (target == null)
        {
            CancelRevive();
            return;
        }

        if (!Input.GetKey(reviveKey))
        {
            CancelRevive();
            return;
        }

        if (target.IsBeingRevived && target.ReviverViewID != photonView.ViewID)
        {
            CancelRevive();
            return;
        }

        if (reviveTarget != target)
        {
            StartRevive(target);
        }

        if (!isReviving)
            return;

        float distance = Vector2.Distance(transform.position,reviveTarget.transform.position);

        if (distance > reviveDistance)
        {
            CancelRevive();
            return;
        }

        reviveProgress += Time.deltaTime;

        if (reviveTargetUI != null)
        {
            reviveTargetUI.SetReviveProgress(reviveProgress / reviveTime);
        }

        if (reviveProgress >= reviveTime)
        {
            FinishRevive();
        }
    }

    private PlayerManager FindDownedPlayer()
    {
        PlayerManager closest = null;

        float closestDistance = reviveDistance;

        foreach (PlayerManager player in GameManager.Instance.Players)
        {
            if (player == null)
                continue;

            if (player == playerManager)
                continue;

            if (!player.IsDowned())
                continue;

            float distance = Vector2.Distance(transform.position,player.transform.position);

            if (distance <= closestDistance)
            {
                closest = player;
                closestDistance = distance;
            }
        }

        return closest;
    }

    private void StartRevive(PlayerManager target)
    {
        photonView.RPC(nameof(RPC_RequestStartRevive),RpcTarget.MasterClient,target.photonView.ViewID);

        //reviveTarget = target;
        //reviveTargetUI = target.GetComponentInChildren<PlayerUI>();
        //reviveProgress = 0f;
        //isReviving = true;

        //if (playerUI != null)
        //{
        //    playerUI.ShowReviveProgress();
        //    playerUI.SetReviveProgress(0f);
        //}

    }

    private void CancelRevive()
    {
        if (!isReviving)
            return;

        if (reviveTarget != null)
        {
            photonView.RPC(nameof(RPC_RequestCancelRevive),RpcTarget.MasterClient,reviveTarget.photonView.ViewID);
        }

        if (reviveTargetUI != null)
        {
            reviveTargetUI.HideReviveProgress();
        }

        reviveTarget = null;
        reviveTargetUI = null;
        reviveProgress = 0f;
        isReviving = false;

    }

    private void FinishRevive()
    {
        if (reviveTarget == null)
            return;

        //reviveTarget.Revive();

        photonView.RPC(nameof(RPC_RequestFinishRevive),RpcTarget.MasterClient,reviveTarget.photonView.ViewID);

        if (reviveTargetUI != null)
        {
            reviveTargetUI.HideReviveProgress();
        }

        reviveTarget = null;
        reviveTargetUI = null;
        reviveProgress = 0f;
        isReviving = false;
    }

    [PunRPC]
    private void RPC_StartReviveResult(bool accepted, int targetViewID)
    {
        if (!accepted)
            return;

        PhotonView targetView = PhotonView.Find(targetViewID);

        if (targetView == null)
            return;

        reviveTarget = targetView.GetComponent<PlayerManager>();

        reviveTargetUI = reviveTarget.GetComponentInChildren<PlayerUI>();

        reviveProgress = 0f;
        isReviving = true;

        if (reviveTargetUI != null)
        {
            reviveTargetUI.ShowReviveProgress();
            reviveTargetUI.SetReviveProgress(0f);
        }
    }

    [PunRPC]
    private void RPC_RequestStartRevive(int targetViewID)
    {
        PhotonView targetView = PhotonView.Find(targetViewID);

        if (targetView == null)
            return;

        PlayerManager target = targetView.GetComponent<PlayerManager>();

        if (target == null)
            return;

        //target.StartBeingRevived(photonView.ViewID);
        bool accepted = target.StartBeingRevived(photonView.ViewID);
        photonView.RPC(nameof(RPC_StartReviveResult),photonView.Owner,accepted,targetViewID);
    }

    [PunRPC]
    private void RPC_RequestCancelRevive(int targetViewID)
    {
        PhotonView targetView = PhotonView.Find(targetViewID);

        if (targetView == null)
            return;

        PlayerManager target = targetView.GetComponent<PlayerManager>();

        if (target == null)
            return;

        target.CancelBeingRevived(photonView.ViewID);
    }

    [PunRPC]
    private void RPC_RequestFinishRevive(int targetViewID)
    {
        PhotonView targetView = PhotonView.Find(targetViewID);

        if (targetView == null)
            return;

        PlayerManager target = targetView.GetComponent<PlayerManager>();

        if (target == null)
            return;

        target.FinishBeingRevived(photonView.ViewID);
    }
}
