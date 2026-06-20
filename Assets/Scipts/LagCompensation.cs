using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LagCompensation : MonoBehaviourPun, IPunObservable
{
    private Vector2 networkPosition;
    private Quaternion networkRotation; // Guardará la rotación que viene de red

    private Rigidbody2D rb;

    [Tooltip("Velocidad de suavizado para posición y rotación.")]
    public float smoothingSpeed = 15f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Inicializamos para evitar saltos raros al spawnear
        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        // Si NO es mi personaje (es el rival), aplicamos interpolación de red
        if (!photonView.IsMine)
        {
            // 1. Interpolación de posición en el Rigidbody
            Vector2 lerpedPosition = Vector2.Lerp(rb.position, networkPosition, Time.fixedDeltaTime * smoothingSpeed);
            rb.MovePosition(lerpedPosition);

            // 2. Interpolación de rotación directa en el Transform
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.fixedDeltaTime * smoothingSpeed);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // NUESTRA PANTALLA: Enviamos posición física y luego rotación estructural
            stream.SendNext(rb.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // PANTALLA DEL RIVAL: Leemos EN EL MISMO ORDEN EXACTO en el que se envió
            networkPosition = (Vector2)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}