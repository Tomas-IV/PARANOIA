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
    private Collider2D[] colliders;

    private void Start()
    {
        movement = GetComponent<PlayerMovement>();
        colliders = GetComponents<Collider2D>();

        CurrentHealth = maxHealth;
        LifeState = PlayerLifeState.Alive;

        GameManager.Instance.Players.Add(this);

        TryGetTeam();
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.Players.Remove(this);
    }

    private void TryGetTeam()
    {
        if (photonView.Owner.CustomProperties != null &&
            photonView.Owner.CustomProperties.TryGetValue("team", out object team))
        {
            Team = (int)team;
            Debug.Log($"[TEAM] {photonView.Owner.NickName} = Team {Team}");
        }
        else
        {
            Debug.LogWarning($"[TEAM] Not ready for {photonView.Owner.NickName}, retrying...");
            Invoke(nameof(TryGetTeam), 0.5f);
        }
    }

    public void TakeDamage(int damage, int attackerTeam)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        if (LifeState != PlayerLifeState.Alive)
            return;

        if (attackerTeam == Team)
        {
            Debug.Log($"[DAMAGE] FIRE FRIENDLY BLOCKED on {photonView.Owner.NickName}");
            return;
        }

        Debug.Log($"[DAMAGE] {photonView.Owner.NickName} took {damage}");

        ApplyDamage(damage);
    }

    private void ApplyDamage(int damage)
    {
        CurrentHealth -= damage;

        Debug.Log($"[DAMAGE] HP after: {CurrentHealth}");

        photonView.RPC(nameof(RPC_UpdateHealth), RpcTarget.All, CurrentHealth);

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

        if (PhotonNetwork.IsMasterClient)
            GameManager.Instance.CheckWinCondition();
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

    public bool IsAlive() => LifeState == PlayerLifeState.Alive;
    public bool IsDowned() => LifeState == PlayerLifeState.Downed;
}
