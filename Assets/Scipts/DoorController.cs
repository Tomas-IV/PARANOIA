using UnityEngine;
using Photon.Pun;

public class DoorController : MonoBehaviourPun
{
    [Header("Puerta Abajo")]
    public GameObject puertaAbajo; // Aca va Door (1)

    [Header("Ajustes de Movimiento")]
    public float casillasAMover = 3f; // Cuánto se van a mover
    public float velocidadMovimiento = 2f; // Qué tan rápido se deslizan

    private bool boton1Activo = false;
    private bool boton2Activo = false;
    private bool abrirPuertas = false;

    // Guardamos las coordenadas matemáticas
    private Vector3 posInicialPrincipal;
    private Vector3 posDestinoPrincipal;

    private Vector3 posInicialAbajo;
    private Vector3 posDestinoAbajo;

    void Start()
    {
        // Al arrancar, memorizamos dónde están y calculamos a dónde tienen que ir
        posInicialPrincipal = transform.position;
        // La puerta principal (arriba) se mueve hacia ARRIBA en el eje Y
        posDestinoPrincipal = posInicialPrincipal + new Vector3(0, casillasAMover, 0);

        if (puertaAbajo != null)
        {
            posInicialAbajo = puertaAbajo.transform.position;
            // La puerta secundaria (abajo) se mueve hacia ABAJO en el eje Y
            posDestinoAbajo = posInicialAbajo + new Vector3(0, -casillasAMover, 0);
        }
    }

    public void EnviarVoto(int id, bool estado)
    {
        photonView.RPC(nameof(RPC_RecibirVoto), RpcTarget.AllBuffered, id, estado);
    }

    [PunRPC]
    void RPC_RecibirVoto(int id, bool estado)
    {
        if (abrirPuertas) return; // Si ya se están abriendo, bloqueamos los botones

        if (id == 1) boton1Activo = estado;
        if (id == 2) boton2Activo = estado;

        // Si VOS y TU COMPAÑERO mantienen Q...
        if (boton1Activo && boton2Activo)
        {
            abrirPuertas = true;
        }
    }

    void Update()
    {
        // Si el candado se abrió, movemos las puertas físicamente en cada frame
        if (abrirPuertas)
        {
            // Desliza esta puerta hacia arriba
            transform.position = Vector3.MoveTowards(transform.position, posDestinoPrincipal, velocidadMovimiento * Time.deltaTime);

            // Desliza la otra puerta hacia abajo
            if (puertaAbajo != null)
            {
                puertaAbajo.transform.position = Vector3.MoveTowards(puertaAbajo.transform.position, posDestinoAbajo, velocidadMovimiento * Time.deltaTime);
            }
        }
    }
}