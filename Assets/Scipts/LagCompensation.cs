using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class LagCompensation : MonoBehaviourPun, IPunObservable
{
    private Vector2 networkPosition;
    private float networkRotation;

    private Rigidbody2D rb;
    public float smoothingSpeed = 15f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        networkPosition = rb.position;
        networkRotation = transform.eulerAngles.z;
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            Vector2 lerpedPosition =
                Vector2.Lerp(rb.position, networkPosition,
                Time.fixedDeltaTime * smoothingSpeed);

            rb.MovePosition(lerpedPosition);

            float lerpedRotation =
                Mathf.LerpAngle(rb.rotation, networkRotation,
                Time.fixedDeltaTime * smoothingSpeed);

            rb.MoveRotation(lerpedRotation);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(rb.position);

            // Enviamos la rotación real del transform
            stream.SendNext(transform.eulerAngles.z);
        }
        else
        {
            networkPosition = (Vector2)stream.ReceiveNext();
            networkRotation = (float)stream.ReceiveNext();
        }
    }
}