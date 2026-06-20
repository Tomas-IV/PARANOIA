using UnityEngine;
using Photon.Pun;

public class LagCompensation : MonoBehaviourPun, IPunObservable
{
    private Vector2 networkPosition;
    private Quaternion networkRotation;

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
        // Si NO es mi personaje,interpolacion
        if (!photonView.IsMine)
        {
            // Suavizamos el desplazamiento
            Vector2 lerpedPosition = Vector2.Lerp(rb.position, networkPosition, Time.fixedDeltaTime * smoothingSpeed);
            rb.MovePosition(lerpedPosition);

            // Suavizamos el giro
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.fixedDeltaTime * smoothingSpeed);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Si el personaje es tuyo, mandas tu posicion y tu rotacion actual a la red
            stream.SendNext(rb.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // Si es el rival, lees su posicion y su rotacion en el mismo orden exacto
            networkPosition = (Vector2)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}