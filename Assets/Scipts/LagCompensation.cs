using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class LagCompensation : MonoBehaviourPun, IPunObservable
{
    private Vector2 NetworkPosition;
    private Quaternion NetworkRotation; // <--- NUEVO: Guarda la rotación de red

    private Rigidbody2D rb;

    [Tooltip("Velocidad de suavizado. Más alto = más rápido copia la posición y rotación real.")]
    public float smoothingSpeed = 15f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // Si NO es mi personaje (es un enemigo), interpolamos su movimiento y su rotación
        if (!photonView.IsMine)
        {
            // 1. INTERPOLACIÓN DE POSICIÓN
            Vector2 lerpedPosition = Vector2.Lerp(rb.position, NetworkPosition, Time.fixedDeltaTime * smoothingSpeed);
            rb.MovePosition(lerpedPosition);

            // 2. INTERPOLACIÓN DE ROTACIÓN (NUEVO)
            transform.rotation = Quaternion.Lerp(transform.rotation, NetworkRotation, Time.fixedDeltaTime * smoothingSpeed);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Si el personaje es MÍO, mandamos nuestra posición y nuestra rotación actual a los demás
            stream.SendNext(rb.position);
            stream.SendNext(transform.rotation); // <--- NUEVO
        }
        else
        {
            // Si el personaje es de OTRO, recibimos sus datos reales y los guardamos
            NetworkPosition = (Vector2)stream.ReceiveNext();
            NetworkRotation = (Quaternion)stream.ReceiveNext(); // <--- NUEVO
        }
    }
}