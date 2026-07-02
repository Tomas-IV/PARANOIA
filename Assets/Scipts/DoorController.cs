using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Segunda Puerta Opcional")]
    [SerializeField] private GameObject otraPuerta;

    private HashSet<int> botonesActivados = new HashSet<int>();
    private bool yaSeDesvanecio = false;

    public void EnviarConfirmacionInput(int idBoton)
    {
        // Usamos AllBuffered para asegurar la sincronizacion total e inmediata
        photonView.RPC(nameof(RPC_RegistrarQBoton), RpcTarget.AllBuffered, idBoton);
    }

    [PunRPC]
    private void RPC_RegistrarQBoton(int idBoton)
    {
        if (yaSeDesvanecio) return;

        if (!botonesActivados.Contains(idBoton))
        {
            botonesActivados.Add(idBoton);
            Debug.Log($"[PUERTA] Boton {idBoton} registrado. Total activos: {botonesActivados.Count}/2");
        }

        if (botonesActivados.Count >= 2)
        {
            yaSeDesvanecio = true;
            Debug.Log("[PUERTA] Sincronización exitosa. Desvaneciendo estructuras.");

            if (otraPuerta != null)
            {
                otraPuerta.SetActive(false);
            }

            gameObject.SetActive(false);
        }
    }
}