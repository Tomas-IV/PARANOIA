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

    private PlayerMovement movement;
    private Collider2D[] colliders;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        colliders = GetComponents<Collider2D>();

        CurrentHealth = maxHealth;
        LifeState = PlayerLifeState.Alive;

        GameManager.Instance.Players.Add(this);

    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Players.Remove(this);
    }

    public void TakeDamage(int damage)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (LifeState != PlayerLifeState.Alive)
            return;

        ApplyDamage(damage);

        Debug.Log($"[DAMAGE] {photonView.Owner.NickName} took {damage}");

    }

    private void ApplyDamage(int damage)
    {
        CurrentHealth -= damage;

        photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, CurrentHealth);
        
        Debug.Log($"[DAMAGE] HP after: {CurrentHealth}");

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
    }

    [PunRPC]
    private void RPC_Downed()
    {
        CurrentHealth = 0;
        LifeState = PlayerLifeState.Downed;

        movement.SetCanMove(false);

        SetHitboxActive(false);

        Debug.Log($"{photonView.Owner.NickName} DOWNED");
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

        SetHitboxActive(true);

        Debug.Log($"{photonView.Owner.NickName} REVIVED");
    }

    private void SetHitboxActive(bool active)
    {
        foreach (var col in colliders)
            col.enabled = active;
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

    public bool IsAlive() => LifeState == PlayerLifeState.Alive;
    public bool IsDowned() => LifeState == PlayerLifeState.Downed;
}
