using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CameraFollow : MonoBehaviour
{
    private Transform target;

    public Vector3 offset = new Vector3(0, 0, -10);
    public float smooth = 10f;

    public void SetTarget(Transform t)
    {
        target = t;
    }

    void LateUpdate()
    {
        if (target == null) return;

        transform.position = Vector3.Lerp(transform.position,target.position + offset,smooth * Time.deltaTime);
    }
}
