using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;

public class LagCompensation : MonoBehaviourPun, IPunObservable
{
    // Start is called before the first frame update

    private Vector2 NetworkPosition;
    private Rigidbody2D rb;
    void Start()
    {

        rb = GetComponent<Rigidbody2D>();


    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {

        }
    }


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

    }
}
