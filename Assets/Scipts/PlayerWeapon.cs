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

        RaycastHit2D hit = Physics2D.Raycast(origin,direction,range,hitMask);

        Vector2 hitPoint;

        if (hit.collider != null)
        {
            hitPoint = hit.point;

            if (hit.collider.TryGetComponent(out PlayerManager target))
            {
                if (target.Team != playerManager.Team)
                {
                    photonView.RPC(nameof(RPC_RequestDamage),RpcTarget.MasterClient,target.photonView.ViewID,damage,playerManager.Team);
                }
            }
        }
        else
        {
            hitPoint = origin + direction * range;
        }

        SpawnBulletFX(origin, hitPoint);

        photonView.RPC(nameof(RPC_PlayBulletFX),RpcTarget.Others,origin,hitPoint);
    }

    private void SpawnBulletFX(Vector2 origin, Vector2 hitPoint)
    {
        GameObject bullet =
            Instantiate(bulletVFXPrefab, origin, Quaternion.identity);

        bullet.GetComponent<BulletVisual>()
            .Init(hitPoint, bulletSpeed);
    }

    [PunRPC]
    private void RPC_PlayBulletFX(Vector2 origin, Vector2 hitPoint)
    {
        SpawnBulletFX(origin, hitPoint);
    }

    [PunRPC]
    private void RPC_RequestDamage(int targetViewID,int damage,int attackerTeam)
    {
        PhotonView view = PhotonView.Find(targetViewID);

        if (view == null)
            return;

        PlayerManager target = view.GetComponent<PlayerManager>();

        if (target != null)
        {
            target.TakeDamage(damage, attackerTeam);
        }
    }
}
