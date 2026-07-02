using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerWeapon : MonoBehaviourPun
{
    [SerializeField] private LayerMask hitMask;

    [Header("Visual")]
    [SerializeField] private GameObject bulletVFXPrefab;
    [SerializeField] private Transform firePoint;
    [SerializeField] private Transform reference;
    [SerializeField] private float bulletSpeed = 25f;

    private WeaponStats weaponStats;
    private float nextShootTime;

    private void Start()
    {
        weaponStats = GetComponent<WeaponStats>();

        // Protección: Si nos olvidamos de poner WeaponStats en el Inspector, nos avisa sin romper todo
        if (weaponStats == null)
        {
            Debug.LogError($"¡Falta el componente WeaponStats en el objeto {gameObject.name}! Por favor asignáselo en el Inspector.");
        }
    }

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        // Si weaponStats no se encontró, salimos del Update para evitar el NullReferenceException
        if (weaponStats == null)
            return;

        if (weaponStats.Automatic)
        {
            if (Input.GetMouseButton(0))
                TryShoot();
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                TryShoot();
        }
    }

    private void TryShoot()
    {
        if (Time.time < nextShootTime)
            return;

        nextShootTime = Time.time + weaponStats.FireRate;

        Shoot();
    }

    private void Shoot()
    {
        Debug.Log("DISPARO");
        Vector2 origin = firePoint.position;
        Vector2 direction = reference.right;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction, weaponStats.Range, hitMask);

        Vector2 hitPoint;

        if (hit.collider != null)
        {
            hitPoint = hit.point;

            if (hit.collider.TryGetComponent(out PlayerManager target))
            {
                Debug.Log("Player encontrado");
                photonView.RPC(nameof(RPC_RequestDamage), RpcTarget.MasterClient, target.photonView.ViewID, weaponStats.Damage);
            }
            else if (hit.collider.TryGetComponent(out SeguirJugador enemigo))
            {
                Debug.Log("Zombi detectado por Raycast");
                photonView.RPC(nameof(RPC_RequestEnemyDamage), RpcTarget.MasterClient, enemigo.photonView.ViewID, weaponStats.Damage);
            }
        }
        else
        {
            Debug.Log("No hit");
            hitPoint = origin + direction * weaponStats.Range;
        }

        SpawnBulletFX(origin, hitPoint);
        photonView.RPC(nameof(RPC_PlayBulletFX), RpcTarget.Others, origin, hitPoint);
    }

    private void SpawnBulletFX(Vector2 origin, Vector2 hitPoint)
    {
        Debug.Log("Spawn Bullet");
        GameObject bullet = Instantiate(bulletVFXPrefab, origin, Quaternion.identity);

        bullet.GetComponent<BulletVisual>().Init(hitPoint, bulletSpeed);
        Debug.Log(bullet.name);
    }

    [PunRPC]
    private void RPC_PlayBulletFX(Vector2 origin, Vector2 hitPoint)
    {
        SpawnBulletFX(origin, hitPoint);
    }

    [PunRPC]
    private void RPC_RequestDamage(int targetViewID, int damage)
    {
        PhotonView view = PhotonView.Find(targetViewID);
        if (view == null) return;

        PlayerManager target = view.GetComponent<PlayerManager>();
        if (target != null)
        {
            target.TakeDamage(damage);
        }
    }

    [PunRPC]
    private void RPC_RequestEnemyDamage(int enemyViewID, int damage)
    {
        PhotonView view = PhotonView.Find(enemyViewID);
        if (view == null) return;

        SeguirJugador enemigo = view.GetComponent<SeguirJugador>();
        if (enemigo != null)
        {
            enemigo.RecibirDanio(damage);
        }
    }
}