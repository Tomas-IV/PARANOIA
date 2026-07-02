using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    private HashSet<int> botonesActivados = new HashSet<int>();
    private bool yaSeDesvanecio = false;

    public void EnviarConfirmacionInput(int idBoton)
    {
        // Enviamos la informacion del boton a TODOS los clientes de la sala
        photonView.RPC(nameof(RPC_RegistrarQBoton), RpcTarget.All, idBoton);
    }

    [PunRPC]
    private void RPC_RegistrarQBoton(int idBoton)
    {
        if (yaSeDesvanecio) return;

        // Registramos el boton en las listas de todas las computadoras en simultaneo
        if (!botonesActivados.Contains(idBoton))
        {
            botonesActivados.Add(idBoton);
            Debug.Log("Boton " + idBoton + " registrado. Total botones activos: " + botonesActivados.Count + "/2");
        }

        // Si en la lista local de CADA PC ya figuran ambos botones (el 1 y el 2)
        if (botonesActivados.Count >= 2)
        {
            yaSeDesvanecio = true;
            Debug.Log("¡Ambos botones presionados! Desvaneciendo puertas...");

            // Apagamos el objeto de la puerta de manera local y simultanea
            gameObject.SetActive(false);
        }
    }
}