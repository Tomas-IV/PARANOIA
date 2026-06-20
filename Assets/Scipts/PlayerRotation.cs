using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerRotation : MonoBehaviourPun
{
    void Update()
    {
        //  Si este personaje NO es el mío, frenamos acá. 
        // Esto evita que tu mouse controle al rival.
        if (!photonView.IsMine)
        {
            return;
        }

        // Tu código original de rotación por mouse
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = 0f;

        Vector2 direction = mouseWorld - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}