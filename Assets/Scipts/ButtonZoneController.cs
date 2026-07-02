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

        bool estadoActual = hayJugadorMio && Input.GetKey(KeyCode.Q);

        if (estadoActual != presionandoQ)
        {
            presionandoQ = estadoActual;

            if (puertaPrincipal != null)
            {
                // Acá está la línea corregida con el nombre exacto
                puertaPrincipal.EnviarVoto(idBoton, presionandoQ);
            }
            else
            {
                Debug.LogError($"¡El botón {idBoton} no tiene la puerta asignada en el Inspector!");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}