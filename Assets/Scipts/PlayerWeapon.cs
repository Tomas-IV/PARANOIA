using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerWeapon : MonoBehaviourPun
{
    [SerializeField] private float range = 10f;
    [SerializeField] private int damage = 20;
    [SerializeField] private LayerMask hitMask;

    [Header("Visual")]
    [SerializeField] private GameObject bulletVFXPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform reference;
    [SerializeField] private float bulletSpeed = 25f;

    private PlayerManager playerManager;

    private void Start()
    {
        playerManager = GetComponent<PlayerManager>();
    }

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        if (Input.GetMouseButtonDown(0))
            Shoot();
    }

    private void Shoot()
    {
        Vector2 origin = firePoint.position;
        Vector2 direction = reference.right;

        Debug.Log($"[WEAPON] Shoot from {origin}");

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, range, hitMask);

        Vector2 hitPoint;

        if (hit.collider != null)
        {
            hitPoint = hit.point;

            Debug.Log($"[WEAPON] Hit: {hit.collider.name}");

            if (hit.collider.TryGetComponent(out PlayerManager target))
            {
                Debug.Log($"[TEAM DEBUG] Shooter={playerManager.Team} Target={target.Team}");

                if (target.Team == playerManager.Team)
                {
                    Debug.Log($"[WEAPON] FIRE FRIENDLY BLOCKED");
                    SpawnBulletFX(origin, hitPoint);
                    return;
                }

                photonView.RPC(
                    nameof(RPC_RequestDamage),
                    RpcTarget.MasterClient,
                    target.photonView.ViewID,
                    damage,
                    playerManager.Team
                );
            }
        }
        else
        {
            hitPoint = origin + direction * range;
            Debug.Log("[WEAPON] Miss");
        }

        SpawnBulletFX(origin, hitPoint);
    }

    private void SpawnBulletFX(Vector2 origin, Vector2 hitPoint)
    {
        GameObject bullet = Instantiate(bulletVFXPrefab, origin, Quaternion.identity);
        bullet.GetComponent<BulletVisual>().Init(hitPoint, bulletSpeed);
    }

    [PunRPC]
    private void RPC_RequestDamage(int targetViewID, int damage, int attackerTeam)
    {
        PhotonView view = PhotonView.Find(targetViewID);

        if (view == null)
            return;

        PlayerManager target = view.GetComponent<PlayerManager>();

        if (target != null)
            target.TakeDamage(damage, attackerTeam);
    }
}
