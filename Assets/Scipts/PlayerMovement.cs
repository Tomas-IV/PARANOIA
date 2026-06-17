using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun
{
    public float speed = 5f;

    private bool canMove = true;
    private Rigidbody2D rb;
    private Vector2 input;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetCanMove(bool value)
    {
        canMove = value;
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        input = new Vector2(Input.GetAxisRaw("Horizontal"),Input.GetAxisRaw("Vertical")).normalized;
        //input.x = Input.GetAxisRaw("Horizontal");
        //input.y = Input.GetAxisRaw("Vertical");

        //input = input.normalized;
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (!canMove)return;
        

        Vector2 targetPosition = rb.position + input * speed * Time.fixedDeltaTime;
        rb.MovePosition(targetPosition);
    }
}
