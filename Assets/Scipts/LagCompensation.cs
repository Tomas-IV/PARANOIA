using UnityEngine;
using Photon.Pun;

public class LagCompensation : MonoBehaviourPun, IPunObservable
{
    private Vector2 networkPosition;
    private float networkAngle; // Guardamos el angulo flotante en vez de un Quaternion

    private Rigidbody2D rb;

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
            // Aplicamos posicion y rotacion directamente al Rigidbody de forma instantanea
            rb.position = networkPosition;
            rb.rotation = networkAngle;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Mandamos los datos de nuestro Rigidbody2D a la red
            stream.SendNext(rb.position);
            stream.SendNext(rb.rotation); // Angulo flotante en 2D
        }
        else
        {
            // Recibimos los datos del rival de forma exacta
            networkPosition = (Vector2)stream.ReceiveNext();
            networkAngle = (float)stream.ReceiveNext();
        }
    }
}