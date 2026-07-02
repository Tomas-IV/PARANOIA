using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Granade : MonoBehaviourPun, IPunObservable
{
    [Header("Movement")]
    [SerializeField] private float speed = 8f;

    [Header("Explosion")]
    [SerializeField] private float explodeDelay = 2f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private int damage = 50;

    private Vector3 networkPosition;
    private Vector3 targetPosition;
    private bool initialized;
    private float timer;
    private int ownerActorNumber;

    public void Initialize(Vector3 destination, int actorNumber)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        targetPosition = destination;
        ownerActorNumber = actorNumber;
        initialized = true;
    }

    private void Update()
    {
        if (!initialized)
            return;

        if (PhotonNetwork.IsMasterClient)
        {
            UpdateMovement();

            timer += Time.deltaTime;

            if (timer >= explodeDelay)
            {
                Explode();
            }
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position,networkPosition,15f * Time.deltaTime);
        }
    }

    private void UpdateMovement()
    {
        transform.position = Vector3.MoveTowards(transform.position,targetPosition,speed * Time.deltaTime);
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            SeguirJugador enemy = hit.GetComponent<SeguirJugador>();

            if (enemy != null)
            {
                enemy.photonView.RPC(nameof(SeguirJugador.RecibirDanioRed),RpcTarget.MasterClient,damage,photonView.OwnerActorNr);

                continue;
            }

            PlayerManager player = hit.GetComponent<PlayerManager>();

            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }

        PhotonNetwork.Destroy(gameObject);
    }

    public void OnPhotonSerializeView(
        PhotonStream stream,
        PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
        }
        else
        {
            networkPosition = (Vector3)stream.ReceiveNext();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
