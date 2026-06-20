using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LagCompensation : MonoBehaviourPun, IPunObservable
{
    private Vector2 networkPosition;
    private Rigidbody2D rb;
    public float smoothingSpeed = 15f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        networkPosition = transform.position;
    }

    private void FixedUpdate()
    {
        // Si es el rival, suavizamos su posición para ganarle al lag
        if (!photonView.IsMine)
        {
            Vector2 lerpedPosition = Vector2.Lerp(rb.position, networkPosition, Time.fixedDeltaTime * smoothingSpeed);
            rb.MovePosition(lerpedPosition);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position); // Enviamos nuestra posición
        }
        else
        {
            networkPosition = (Vector2)stream.ReceiveNext(); // Recibimos la posición del rival
        }
    }
}