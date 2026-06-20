using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class LagCompensation : MonoBehaviourPun, IPunObservable
{
    private Vector2 NetworkPosition;
    private Rigidbody2D rb;

    [Tooltip("Velocidad de suavizado. Más alto = más rápido copia la posición real, pero puede verse más brusco.")]
    public float smoothingSpeed = 15f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // Si NO es mi personaje (es un enemigo), interpolamos su posición
        if (!photonView.IsMine)
        {
            // INTERPOLACIÓN: Movemos el Rigidbody suavemente hacia la última posición de red recibida
            Vector2 lerpedPosition = Vector2.Lerp(rb.position, NetworkPosition, Time.fixedDeltaTime * smoothingSpeed);
            rb.MovePosition(lerpedPosition);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Si el personaje es MÍO, mandamos nuestra posición real a los demás
            stream.SendNext(rb.position);
        }
        else
        {
            // Si el personaje es de OTRO, recibimos su posición y la guardamos en el "punto real"
            NetworkPosition = (Vector2)stream.ReceiveNext();
        }
    }
}