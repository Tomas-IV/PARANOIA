using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!photonView.IsMine) return;

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

    public void SetCanMove(bool state)
    {
        canMove = state;

        if (!canMove)
        {
            rb.velocity = Vector2.zero;
        }
    }

    public bool CanMove()
    {
        return canMove;
    }
}