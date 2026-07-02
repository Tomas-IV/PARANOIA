using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerManager : MonoBehaviourPun
{
    public enum PlayerLifeState
    {
        Alive,
        Downed
    }

    [Header("Health")]
    [SerializeField] private int maxHealth = 100;
    private PlayerUI playerUI;
    private PlayerMovement movement;
    private Collider2D[] colliders;

    public int CurrentHealth { get; private set; }
    public int MaxHealth => maxHealth;
    public PlayerLifeState LifeState { get; private set; }

    public bool IsBeingRevived { get; private set; }
    public int ReviverViewID { get; private set; } = 1;


    private void Awake()
    {
        playerUI = GetComponentInChildren<PlayerUI>();
    }

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        colliders = GetComponents<Collider2D>();

        CurrentHealth = maxHealth;
        LifeState = PlayerLifeState.Alive;
        IsBeingRevived = false;
        ReviverViewID = -1;

        if (playerUI != null)
        {
            playerUI.SetHealth(CurrentHealth, maxHealth);
            playerUI.SetDowned(false);
        }


        if (GameManager.Instance != null && GameManager.Instance.Players != null)
        {
            GameManager.Instance.Players.Add(this);
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null && GameManager.Instance.Players != null)
            GameManager.Instance.Players.Remove(this);
    }

    public void TakeDamage(int damage)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (LifeState != PlayerLifeState.Alive)
            return;

        ApplyDamage(damage);

        //Debug.Log("[DAMAGE] " + photonView.Owner.NickName + " took " + damage);
    }

    private void ApplyDamage(int damage)
    {
        CurrentHealth -= damage;

        photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, CurrentHealth);

        Debug.Log("[DAMAGE] HP after: " + CurrentHealth);

        if (CurrentHealth <= 0)
            EnterDownedState();
    }

    private void EnterDownedState()
    {
        photonView.RPC(nameof(RPC_Downed), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_UpdateHealth(int health)
    {
        CurrentHealth = health;

        // CORRECCION: Validacion para que no tire NullReference en clientes remotos
        if (playerUI != null)
        {
            playerUI.SetHealth(CurrentHealth, maxHealth);
        }
    }

    [PunRPC]
    private void RPC_Downed()
    {
        CurrentHealth = 0;
        LifeState = PlayerLifeState.Downed;
        IsBeingRevived = false;
        ReviverViewID = -1;

        if (movement != null)
        {
            movement.SetCanMove(false);
        }

        SetHitboxActive(false);

        if (playerUI != null)
        {
            playerUI.SetDowned(true);
        }

        Debug.Log(photonView.Owner.NickName + " DOWNED");
    }

    public void Revive()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        photonView.RPC(nameof(RPC_Revive), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_Revive()
    {
        CurrentHealth = maxHealth / 2;
        LifeState = PlayerLifeState.Alive;
        IsBeingRevived = false;
        ReviverViewID = -1;

        if (movement != null)
        {
            movement.SetCanMove(true);
        }

        SetHitboxActive(true);

        if (playerUI != null)
        {
            playerUI.SetDowned(false);
            playerUI.SetHealth(CurrentHealth, maxHealth);
        }

        Debug.Log(photonView.Owner.NickName + " REVIVED");
    }

    private void SetHitboxActive(bool active)
    {
        if (colliders == null) return;

        foreach (var col in colliders)
        {
            if (col != null)
            {
                col.enabled = active;
            }
        }
    }

    public void Heal(int amount)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (LifeState != PlayerLifeState.Alive)
            return;

        CurrentHealth += amount;

        if (CurrentHealth > maxHealth)
            CurrentHealth = maxHealth;

        photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, CurrentHealth);
    }

    public bool StartBeingRevived(int reviverViewID)
    {
        if (!PhotonNetwork.IsMasterClient)
            return false;

        if (LifeState != PlayerLifeState.Downed)
            return false;

        if (IsBeingRevived)
            return false;

        photonView.RPC(nameof(RPC_StartBeingRevived),RpcTarget.All,reviverViewID);

        return true;
    }

    public void CancelBeingRevived(int reviverViewID)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!IsBeingRevived)
            return;

        if (ReviverViewID != reviverViewID)
            return;

        photonView.RPC(nameof(RPC_CancelBeingRevived),RpcTarget.All);
    }
    public void FinishBeingRevived(int reviverViewID)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (!IsBeingRevived)
            return;

        if (ReviverViewID != reviverViewID)
            return;

        Revive();
    }

    [PunRPC]
    private void RPC_StartBeingRevived(int reviverViewID)
    {
        IsBeingRevived = true;
        ReviverViewID = reviverViewID;

        Debug.Log(photonView.Owner.NickName +
            " comenzó a ser revivido.");
    }

    [PunRPC]
    private void RPC_CancelBeingRevived()
    {
        IsBeingRevived = false;
        ReviverViewID = -1;

        Debug.Log(photonView.Owner.NickName +
            " dejó de ser revivido.");
    }

    public bool IsAlive() => LifeState == PlayerLifeState.Alive;
    public bool IsDowned() => LifeState == PlayerLifeState.Downed;
}