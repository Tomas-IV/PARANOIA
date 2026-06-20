using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool canMove = true; // Variable interna que usa PlayerManager

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

        // Si el chat esta activo O el PlayerManager te desactivo el movimiento, nos frenamos
        if (GameplayChat.ChatActivo || !canMove)
        {
            moveInput = Vector2.zero;
            return;
        }

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine) return;

        if (GameplayChat.ChatActivo || !canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = moveInput * speed;
    }

    // Esta es la funcion que te pide el PlayerManager para solucionar el error de la imagen
    public void SetCanMove(bool state)
    {
        canMove = state;
    }
}