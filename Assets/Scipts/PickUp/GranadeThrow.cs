using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class GranadeThrow : MonoBehaviourPun
{
    [Header("References")]
    [SerializeField] private GameObject grenadePrefab;
    [SerializeField] private Transform throwPoint;

    [Header("Throw")]
    [SerializeField] private float maxThrowDistance = 6f;

    private PlayerItems playerItems;

    private bool isAiming;

    private Vector3 targetPosition;

    private void Awake()
    {
        playerItems = GetComponent<PlayerItems>();
    }

    private void Update()
    {
        if (!photonView.IsMine)
            return;

        HandleThrow();
    }

    private void HandleThrow()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (!playerItems.HasGrenades())
                return;

            isAiming = true;
        }

        if (isAiming)
        {
            UpdateAim();
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (!isAiming)
                return;

            ThrowGrenade();
        }
    }

    private void UpdateAim()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        Vector3 direction = mousePosition - throwPoint.position;

        if (direction.magnitude > maxThrowDistance)
        {
            direction = direction.normalized * maxThrowDistance;
        }

        targetPosition = throwPoint.position + direction;

        // Más adelante vamos a dibujar el indicador aquí.
    }

    private void ThrowGrenade()
    {
        Debug.Log("ThrowGrenade");
        isAiming = false;

        if (!playerItems.ConsumeGrenade())
            return;

        photonView.RPC(nameof(RPC_RequestThrowGrenade),RpcTarget.MasterClient,throwPoint.position,targetPosition);
    }

    [PunRPC]
    private void RPC_RequestThrowGrenade(Vector3 startPosition, Vector3 destination)
    {
        Debug.Log("RPC_RequestThrowGrenade");
        if (!PhotonNetwork.IsMasterClient)
            return;

        GameObject grenadeObject = PhotonNetwork.InstantiateRoomObject(grenadePrefab.name,startPosition,Quaternion.identity);
        Debug.Log(grenadeObject);
        Granade granade = grenadeObject.GetComponent<Granade>();

        if (granade != null)
        {
            granade.Initialize(destination,photonView.OwnerActorNr);
        }
    }
}
