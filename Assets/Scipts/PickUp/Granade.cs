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

    private Vector3 targetPosition;

    private bool initialized;

    private float timer;

    // Posición que reciben los clientes
    private Vector3 networkPosition;

    public void Initialize(Vector3 destination)
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        targetPosition = destination;

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
        PhotonNetwork.Destroy(gameObject);
    }

    public void OnPhotonSerializeView(PhotonStream stream,PhotonMessageInfo info)
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
}
