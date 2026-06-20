using UnityEngine;
using Photon.Pun;

public class LagCompensation : MonoBehaviourPun, IPunObservable
{
    private Vector2 networkPosition;
    private float networkAngle;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        networkPosition = transform.position;
        networkAngle = rb.rotation;

        // Desactivamos la simulacion de fisicas locales para el rival azul
        if (!photonView.IsMine && rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.simulated = true;
            rb.constraints = RigidbodyConstraints2D.None;
        }
    }

    private void FixedUpdate()
    {
        // Si NO es mi personaje (es el rival azul)
        if (!photonView.IsMine)
        {
            // Forzamos la posicion y la rotacion directo al Rigidbody
            rb.position = networkPosition;
            rb.rotation = networkAngle;

            // Por si las dudas la fisica aun pise el transform, lo sincronizamos directo aqui
            transform.position = networkPosition;
            transform.eulerAngles = new Vector3(0, 0, networkAngle);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // IMPORTANTE: Mandamos el angulo real del Transform por si tu script de rotacion no usa el Rigidbody
            stream.SendNext((Vector2)transform.position);
            stream.SendNext(transform.eulerAngles.z);
        }
        else
        {
            // Recibimos los datos exactos del rival
            networkPosition = (Vector2)stream.ReceiveNext();
            networkAngle = (float)stream.ReceiveNext();
        }
    }
}