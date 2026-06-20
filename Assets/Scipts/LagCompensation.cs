using UnityEngine;
using Photon.Pun;

public class LagCompensation : MonoBehaviourPun, IPunObservable
{
    private Vector2 networkPosition;
    private float networkAngle; // Guardamos el angulo flotante en vez de un Quaternion

    private Rigidbody2D rb;
    public float smoothingSpeed = 15f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        networkPosition = transform.position;
        networkAngle = rb.rotation;
    }

    private void FixedUpdate()
    {
        // Si NO es mi personaje (es el rival azul)
        if (!photonView.IsMine)
        {
            // 1. Desplazamiento fluido (Posicion)
            Vector2 lerpedPosition = Vector2.Lerp(rb.position, networkPosition, Time.fixedDeltaTime * smoothingSpeed);
            rb.MovePosition(lerpedPosition);

            // 2. Giro fluido (Rotacion) usando la fisica de Unity para 2D
            float lerpedAngle = Mathf.LerpAngle(rb.rotation, networkAngle, Time.fixedDeltaTime * smoothingSpeed);
            rb.MoveRotation(lerpedAngle);
        }
    }

    // El tubo de red de Photon
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Si el personaje es tuyo, mandas posicion y angulo actual
            stream.SendNext(rb.position);
            stream.SendNext(transform.eulerAngles.z); // Mandamos el angulo real de tu pantalla
        }
        else
        {
            // Si es el rival, recibis los datos en el mismo orden exacto
            networkPosition = (Vector2)stream.ReceiveNext();
            networkAngle = (float)stream.ReceiveNext();
        }
    }
}