using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Configuración de Movimiento")]
    public float casillasAMover = 3f;
    public float velocidadMovimiento = 2f;

    [Tooltip("Escribí 1 para que suba, o -1 para que baje")]
    public float direccionY = 1f;

    private bool boton1Activo = false;
    private bool boton2Activo = false;
    private bool abrirPuertas = false;

    private Vector3 posInicial;
    private Vector3 posDestino;

    void Start()
    {
        posInicial = transform.position;
        // Cada puerta calcula su propio destino de forma independiente usando su direcciónY
        posDestino = posInicial + new Vector3(0, casillasAMover * direccionY, 0);
    }

    public void EnviarVoto(int id, bool estado)
    {
        // Cada puerta corre su propio RPC en red de forma independiente
        photonView.RPC(nameof(RPC_RecibirVoto), RpcTarget.AllBuffered, id, estado);
    }

    [PunRPC]
    void RPC_RecibirVoto(int id, bool estado)
    {
        if (abrirPuertas) return;

        if (id == 1) boton1Activo = estado;
        if (id == 2) boton2Activo = estado;

        // Si ambos personajes mantienen la Q al mismo tiempo
        if (boton1Activo && boton2Activo)
        {
            abrirPuertas = true;
            Debug.Log($"[PUERTA] {gameObject.name} activada. Iniciando movimiento.");
        }
    }

    void Update()
    {
        if (abrirPuertas)
        {
            // Cada estructura se desplaza suavemente hacia su propia meta calculada
            transform.position = Vector3.MoveTowards(transform.position, posDestino, velocidadMovimiento * Time.deltaTime);
        }
    }
}