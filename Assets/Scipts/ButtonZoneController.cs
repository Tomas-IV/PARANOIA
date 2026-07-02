using UnityEngine;
using Photon.Pun;

public class ButtonZoneController : MonoBehaviourPun
{
    [Header("Referencias")]
    [SerializeField] private DoorController puertaObjetivo;

    [Header("Configuracion de Equipo")]
    [SerializeField] private int idBotonUnico;

    [Header("Configuracion del Sensor")]
    [SerializeField] private Vector2 tamanoSensor = new Vector2(0.39f, 0.30f);

    private bool qEstaPresionada = false;

    private void Update()
    {
        Collider2D[] colisiones = Physics2D.OverlapBoxAll(transform.position, tamanoSensor, 0f);
        bool miJugadorEstaAca = false;

        foreach (Collider2D col in colisiones)
        {
            if (col.CompareTag("Player") || col.gameObject.name.Contains("PlayerSho") || col.gameObject.name.Contains("PlayerSpe"))
            {
                // Buscamos el PhotonView en el objeto, y si está en un hijo, lo buscamos en el padre
                PhotonView pv = col.GetComponent<PhotonView>();
                if (pv == null)
                {
                    pv = col.GetComponentInParent<PhotonView>();
                }

                // Si encontramos tu conexión, te damos permiso
                if (pv != null && pv.IsMine)
                {
                    miJugadorEstaAca = true;
                    break;
                }
            }
        }

        // Si VOS estas parado encima y MANTENÉS apretada la Q
        if (miJugadorEstaAca && Input.GetKey(KeyCode.Q))
        {
            if (!qEstaPresionada)
            {
                qEstaPresionada = true;
                if (puertaObjetivo != null) puertaObjetivo.ActualizarEstadoBoton(idBotonUnico, true);
            }
        }
        // Si soltás la tecla, o si te movés fuera del botón
        else
        {
            if (qEstaPresionada)
            {
                qEstaPresionada = false;
                if (puertaObjetivo != null) puertaObjetivo.ActualizarEstadoBoton(idBotonUnico, false);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, tamanoSensor);
    }
}