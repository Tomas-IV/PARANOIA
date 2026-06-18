using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
//using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks
{
    private PlayerMovement movement;
    private PlayerManager playerManager;

    [Header("Revive")]
    [SerializeField] private float reviveDistance = 2f;
    [SerializeField] private float reviveTime = 5f;

    private PlayerManager reviveTarget;
    private float reviveProgress;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        playerManager = GetComponent<PlayerManager>();

        if (!photonView.IsMine)
        {
            movement.enabled = false;
            return;
        }

        Camera.main.GetComponent<CameraFollow>().SetTarget(transform);
    }

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        HandleReviveInput();
    }

    private void HandleReviveInput()
    {
        PlayerManager target = FindDownedAlly();

        if (target == null)
        {
            ResetRevive();
            return;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            if (reviveTarget != target)
            {
                reviveTarget = target;
                reviveProgress = 0f;
            }

            reviveProgress += Time.deltaTime;

            if (reviveProgress >= reviveTime)
            {
                target.Revive();
                ResetRevive();
            }
        }
        else
        {
            ResetRevive();
        }
    }

    private PlayerManager FindDownedAlly()
    {
        foreach (PlayerManager player in GameManager.Instance.Players)
        {
            if (player == null)
                continue;

            if (player == playerManager)
                continue;

            if (player.Team != playerManager.Team)
                continue;

            if (!player.IsDowned())
                continue;

            float dist = Vector2.Distance(transform.position, player.transform.position);

            if (dist <= reviveDistance)
                return player;
        }

        return null;
    }

    private void ResetRevive()
    {
        reviveTarget = null;
        reviveProgress = 0f;
    }
}
