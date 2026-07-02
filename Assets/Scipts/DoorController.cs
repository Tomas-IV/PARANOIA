using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Ajustes de Apertura")]
    public float distanciaApertura = 3f; // Cuántas casillas se mueve hacia arriba
    public float velocidad = 2.5f;

    private bool boton1Listo = false;
    private bool boton2Listo = false;
    private bool abriendo = false;

    private Vector3 posicionCerrada;
    private Vector3 posicionAbierta;

    private void Start()
    {
        posicionCerrada = transform.position;
        // Se moverá hacia arriba en el eje Y
        posicionAbierta = posicionCerrada + new Vector3(0, distanciaApertura, 0);
    }

    public void EnviarSeñal(int idBoton, bool apretado)
    {
        // Enviamos la orden a todas las computadoras conectadas
        photonView.RPC(nameof(RPC_SincronizarBoton), RpcTarget.AllBuffered, idBoton, apretado);
    }

    [PunRPC]
    private void RPC_SincronizarBoton(int idBoton, bool apretado)
    {
        if (abriendo) return; // Si ya se está abriendo, ignoramos

        if (idBoton == 1) boton1Listo = apretado;
        if (idBoton == 2) boton2Listo = apretado;

        Debug.Log($"[PUERTA] Estado actual -> Botón 1: {boton1Listo} | Botón 2: {boton2Listo}");

        // ¡CONDICIÓN DE ORO! Si ambos están presionando la Q en este instante
        if (boton1Listo && boton2Listo)
        {
            abriendo = true;
            Debug.Log("[PUERTA] ¡LOS DOS BOTONES ACTIVOS! Abriendo puerta...");
        }
    }

    private void Update()
    {
        // Desliza la puerta suavemente hasta su posición abierta
        if (abriendo)
        {
            transform.position = Vector3.MoveTowards(transform.position, posicionAbierta, velocidad * Time.deltaTime);
        }
    }
}