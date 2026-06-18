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

    public int CurrentHealth { get; private set; }
    public PlayerLifeState LifeState { get; private set; }
    public int Team { get; private set; }

    private PlayerMovement movement;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();

        CurrentHealth = maxHealth;
        LifeState = PlayerLifeState.Alive;

        Team = GameManager.Instance.GetPlayerTeam(photonView.Owner);

        GameManager.Instance.Players.Add(this);
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Players.Remove(this);
        }
    }

    public void TakeDamage(int damage)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (LifeState != PlayerLifeState.Alive)
            return;

        ApplyDamage(damage);
    }

    private void ApplyDamage(int damage)
    {
        CurrentHealth -= damage;

        photonView.RPC(
            nameof(RPC_UpdateHealth),
            RpcTarget.All,
            CurrentHealth
        );

        if (CurrentHealth <= 0)
        {
            EnterDownedState();
        }
    }

    private void EnterDownedState()
    {
        photonView.RPC(nameof(RPC_Downed), RpcTarget.All);
    }

    [PunRPC]
    private void RPC_UpdateHealth(int health)
    {
        CurrentHealth = health;
    }

    [PunRPC]
    private void RPC_Downed()
    {
        CurrentHealth = 0;
        LifeState = PlayerLifeState.Downed;

        movement.SetCanMove(false);

        Debug.Log($"{photonView.Owner.NickName} DOWNED");

        if (PhotonNetwork.IsMasterClient)
        {
            GameManager.Instance.CheckWinCondition();
        }
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

        movement.SetCanMove(true);

        Debug.Log($"{photonView.Owner.NickName} REVIVED");
    }

    public bool IsAlive() => LifeState == PlayerLifeState.Alive;
    public bool IsDowned() => LifeState == PlayerLifeState.Downed;
}
