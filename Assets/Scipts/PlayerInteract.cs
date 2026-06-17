using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerInteract : MonoBehaviourPun
{
    private PlayerController controller;
    private PlayerController currentTarget;
    private bool isHolding = false;

    public float range = 1.5f;

    void Start()
    {
        controller = GetComponent<PlayerController>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;
        if (controller.IsCaptured) return;
        if (!HasRole()) return;

        HandleInteract();
    }

    bool HasRole()
    {
        return controller.Role != -1;
    }

    void HandleInteract()
    {
        if (Input.GetKey(KeyCode.E))
        {
            PlayerController target = GetClosestPlayer();

            if (target == null) return;

            // SOLO entra una vez
            if (!isHolding)
            {
                isHolding = true;
                currentTarget = target;

                // seeker
                if (controller.Role == 1)
                {
                    target.photonView.RPC("Capture", RpcTarget.All);
                }

                // hider revive
                if (controller.Role == 0 && target.IsCaptured)
                {
                    target.photonView.RPC("StartRevive", RpcTarget.All);
                }
            }
        }
        else
        {
            if (isHolding)
            {
                isHolding = false;

                if (currentTarget != null)
                {
                    currentTarget.photonView.RPC("StopRevive", RpcTarget.All);
                    currentTarget = null;
                }
            }
        }
    }

    PlayerController GetClosestPlayer()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        float minDist = Mathf.Infinity;
        PlayerController closest = null;

        foreach (var other in players)
        {
            if (other == controller) continue;

            float dist = Vector2.Distance(transform.position, other.transform.position);

            if (dist <= range && dist < minDist)
            {
                minDist = dist;
                closest = other;
            }
        }

        return closest;
    }
}
