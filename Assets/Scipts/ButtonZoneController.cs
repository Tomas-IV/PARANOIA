using UnityEngine;
using Photon.Pun;

public class ButtonZoneController : MonoBehaviour
{
    [Header("Conexión")]
    public DoorController puertaPrincipal;
    public int idBoton;
    public float radioDeteccion = 0.6f;

    private bool presionandoQ = false;

    private void Update()
    {
        bool hayJugadorMio = false;

        Collider2D[] colisiones = Physics2D.OverlapCircleAll(
            transform.position,
            radioDeteccion);

        foreach (Collider2D col in colisiones)
        {
            if (!col.CompareTag("Player"))
            {
                continue;
            }

            PhotonView pv = col.GetComponent<PhotonView>();

            if (pv == null)
            {
                pv = col.GetComponentInParent<PhotonView>();
            }

            if (pv != null && pv.IsMine)
            {
                hayJugadorMio = true;
                break;
            }
        }

        // --- DEBUGS NUEVOS ---
        // Este if solo se ejecuta en el instante exacto en que apretás la Q
        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log($"[Botón {idBoton}] ¡Apretaste la Q! ¿Detectó a tu jugador en el área?: {hayJugadorMio}");

            if (!hayJugadorMio)
            {
                Debug.LogWarning($"[Botón {idBoton}] Apretaste Q pero no hay un jugador tuyo en el área. ¡Revisá el Radio Deteccion ({radioDeteccion}) en el Inspector!");
            }
        }

        bool estadoActual = hayJugadorMio && Input.GetKey(KeyCode.Q);

        if (estadoActual != presionandoQ)
        {
            presionandoQ = estadoActual;

            // Este Debug te avisa cada vez que cambia el estado y envía el voto
            Debug.Log($"[Botón {idBoton}] Cambió el estado a: {presionandoQ}. Enviando voto a la puerta...");

            if (puertaPrincipal != null)
            {
                puertaPrincipal.EnviarVoto(idBoton, presionandoQ);
            }
            else
            {
                Debug.LogError($"[Botón {idBoton}] ¡No tiene la puerta asignada en el Inspector!");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}