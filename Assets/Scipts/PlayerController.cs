using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerController : MonoBehaviourPunCallbacks
{
    private PlayerMovement movement;
    private int role = -1;
    private bool isCaptured = false;
    private bool isBeingRevived = false;
    private float reviveStartTime;
    

    public float reviveDuration = 10f;
    public int Role => role;
    public bool IsCaptured => isCaptured;
    public bool IsBeingRevived => isBeingRevived;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();

        if (!photonView.IsMine)
        {
            movement.enabled = false;
            return;
        }

        Camera.main.GetComponent<CameraFollow>().SetTarget(transform);

        TryGetRoleSafe();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        HandleMovement();
        HandleReviveTimer();
    }

    void LateUpdate()
    {
        if (!photonView.IsMine) return;

        UpdateVisibility();
    }

    void HandleMovement()
    {
        //if (isCaptured)
        //{
        //    movement.SetCanMove(false);
        //    return;
        //}

        //if (!HasRole())
        //{
        //    movement.SetCanMove(false);
        //    return;
        //}

        /*switch (GameManager.Instance.currentState)
        {
            case GameManager.GameState.Waiting:
                movement.SetCanMove(false);
                break;

            case GameManager.GameState.Hiding:
                movement.SetCanMove(role == 0);
                break;

            case GameManager.GameState.Searching:
                movement.SetCanMove(true);
                break;

            case GameManager.GameState.End:
                movement.SetCanMove(false);
                break;
        }*/
    }

    void UpdateVisibility()
    {
        //if (role == -1) return;
        //if (GameManager.Instance == null) return;

        //bool hideOthers =
        //    role == 1 &&
        //    GameManager.Instance.currentState == GameManager.GameState.Hiding;

        //PlayerController[] players = FindObjectsOfType<PlayerController>();

        //foreach (var p in players)
        //{
        //    if (p == this) continue;

        //    Renderer[] renderers = p.GetComponentsInChildren<Renderer>();

        //    foreach (var r in renderers)
        //    {
        //        r.enabled = !hideOthers;
        //    }
        //}
    }

    void HandleReviveTimer()
    {
        if (!isCaptured || !isBeingRevived) return;

        float elapsed = (float)PhotonNetwork.Time - reviveStartTime;

        if (elapsed >= reviveDuration)
        {
            photonView.RPC("Revive", RpcTarget.All);
        }
    }

    //role system
    bool HasRole()
    {
        return PhotonNetwork.LocalPlayer.CustomProperties != null &&
               PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("role");
    }

    void TryGetRoleSafe()
    {
        if (HasRole())
            role = (int)PhotonNetwork.LocalPlayer.CustomProperties["role"];
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!photonView.IsMine) return;

        if (targetPlayer == PhotonNetwork.LocalPlayer &&
            changedProps.ContainsKey("role"))
        {
            role = (int)changedProps["role"];
        }
    }

    // capture/revive system
    [PunRPC]
    public void Capture()
    {
        isCaptured = true;
        isBeingRevived = false;

        if (PhotonNetwork.IsMasterClient)
        {
            //GameManager.Instance.CheckGameOver();
        }
    }

    [PunRPC]
    public void StartRevive()
    {
        if (!isCaptured) return;

        isBeingRevived = true;
        reviveStartTime = (float)PhotonNetwork.Time;
    }

    [PunRPC]
    public void StopRevive()
    {
        isBeingRevived = false;
    }

    [PunRPC]
    public void Revive()
    {
        isCaptured = false;
        isBeingRevived = false;
    }

    public float GetRemainingReviveTime()
    {
        if (!isBeingRevived) return reviveDuration;

        float elapsed = (float)PhotonNetwork.Time - reviveStartTime;
        return Mathf.Clamp(reviveDuration - elapsed, 0f, reviveDuration);
    }
}
