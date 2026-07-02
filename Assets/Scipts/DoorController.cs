using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    private HashSet<int> botonesActivados = new HashSet<int>();
    private bool yaSeDesvanecio = false;

    public void EnviarConfirmacionInput(int idBoton)
    {
        photonView.RPC(nameof(RPC_RegistrarQBoton), RpcTarget.All, idBoton);
    }

    [PunRPC]
    private void RPC_RegistrarQBoton(int idBoton)
    {
        if (yaSeDesvanecio) return;

        if (!botonesActivados.Contains(idBoton))
        {
            botonesActivados.Add(idBoton);
            Debug.Log("PUERTA: Registrado Boton " + idBoton + ". Total activos: " + botonesActivados.Count + "/2");
        }

        if (botonesActivados.Count >= 2)
        {
            yaSeDesvanecio = true;
            Debug.Log("PUERTA: ¡Ambos botones presionados con exito! Desvaneciendo objeto.");
            gameObject.SetActive(false);
        }
    }
}