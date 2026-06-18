using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class BulletVisual : MonoBehaviourPun
{
    private Vector3 target;
    private float speed;

    public void Init(Vector3 targetPoint, float moveSpeed)
    {
        target = targetPoint;
        speed = moveSpeed;
    }

    private void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.05f)
        {
            Destroy(gameObject);
        }
    }
}
