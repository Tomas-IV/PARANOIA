using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Segunda Puerta Opcional")]
    [SerializeField] private GameObject otraPuerta;

    private bool boton1Activo = false;
    private bool boton2Activo = false;
    private bool yaSeDesvanecio = false;

    public void ActualizarEstadoBoton(int idBoton, bool estaApretado)
    {
        // El cliente manda su estado directo a todas las computadoras
        photonView.RPC(nameof(RPC_SincronizarBoton), RpcTarget.All, idBoton, estaApretado);
    }

    [PunRPC]
    private void RPC_SincronizarBoton(int idBoton, bool estaApretado)
    {
        if (yaSeDesvanecio) return;

        // Actualizamos el estado del candado
        if (idBoton == 1) boton1Activo = estaApretado;
        if (idBoton == 2) boton2Activo = estaApretado;

        // LA REGLA DE ORO: Tienen que estar los dos en TRUE al mismo instante
        if (boton1Activo && boton2Activo)
        {
            yaSeDesvanecio = true;
            Debug.Log("¡AMBOS BOTONES MANTENIENDO Q! Desvaneciendo...");

            if (otraPuerta != null)
            {
                otraPuerta.SetActive(false);
            }
            gameObject.SetActive(false);
        }
    }
}