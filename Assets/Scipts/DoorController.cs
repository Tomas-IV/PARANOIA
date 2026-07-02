using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Segunda Puerta Opcional")]
    [SerializeField] private GameObject otraPuerta; // Aca vas a arrastrar a Door (1)

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
            Debug.Log("PUERTA CENTRAL: Registrado Boton " + idBoton + ". Total activos: " + botonesActivados.Count + "/2");
        }

        if (botonesActivados.Count >= 2)
        {
            yaSeDesvanecio = true;
            Debug.Log("PUERTA CENTRAL: ¡Ambos botones presionados! Desvaneciendo ambas puertas.");

            // Apagamos la otra puerta si fue asignada
            if (otraPuerta != null)
            {
                otraPuerta.SetActive(false);
            }

            // Apagamos esta misma puerta
            gameObject.SetActive(false);
        }
    }
}