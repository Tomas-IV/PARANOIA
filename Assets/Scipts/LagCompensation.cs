using UnityEngine;
using Photon.Pun;

public class LagCompensation : MonoBehaviourPun, IPunObservable
{
    private Vector2 networkPosition;
    private Quaternion networkRotation; // Guarda la rotacion que viene de internet

    private Rigidbody2D rb;
    public float smoothingSpeed = 15f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        // Si NO es mi personaje (es el rival azul), aplicamos la interpolacion
        if (!photonView.IsMine)
        {
            // 1. Suavizamos el desplazamiento (Posicion)
            Vector2 lerpedPosition = Vector2.Lerp(rb.position, networkPosition, Time.fixedDeltaTime * smoothingSpeed);
            rb.MovePosition(lerpedPosition);

            // 2. Suavizamos el giro (Rotacion) para que veas al rival apuntar
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.fixedDeltaTime * smoothingSpeed);
        }
    }

    // Este es el tubo de red de Photon que manda y recibe los datos constantemente
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Si el personaje es tuyo (rojo), mandas tu posicion y tu rotacion actual a la red
            stream.SendNext(rb.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // Si es el rival (azul), lees su posicion y su rotacion en el mismo orden exacto
            networkPosition = (Vector2)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}