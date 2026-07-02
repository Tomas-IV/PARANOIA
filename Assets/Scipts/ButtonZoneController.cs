using UnityEngine;
using Photon.Pun;

public class ButtonZoneController : MonoBehaviour
{
    [Header("Conexión")]
    public DoorController puertaPrincipal;
    public int idBoton;
    public float radioDeteccion = 0.6f;

    private bool presionandoQ = false;

    void Update()
    {
        bool hayJugadorMio = false;

        // Escaneamos en un círculo alrededor del botón
        Collider2D[] colisiones = Physics2D.OverlapCircleAll(transform.position, radioDeteccion);

        foreach (Collider2D col in colisiones)
        {
            if (col.CompareTag("Player") || col.gameObject.name.Contains("Player"))
            {
                // Buscamos si el jugador es de VOS (tu cliente local)
                PhotonView pv = col.GetComponent<PhotonView>();
                if (pv == null) pv = col.GetComponentInParent<PhotonView>();

                if (pv != null && pv.IsMine)
                {
                    hayJugadorMio = true;
                    break;
                }
            }
        }

        // ¿Estás parado encima y manteniendo la Q?
        bool estadoActual = (hayJugadorMio && Input.GetKey(KeyCode.Q));

        // Solo enviamos la orden por red si hubo un cambio (apretaste o soltaste)
        if (estadoActual != presionandoQ)
        {
            presionandoQ = estadoActual;
            if (puertaPrincipal != null)
            {
                puertaPrincipal.EnviarVoto(idBoton, presionandoQ);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}