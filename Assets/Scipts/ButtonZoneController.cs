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
        // Escaneamos el área verde
        Collider2D[] colisiones = Physics2D.OverlapBoxAll(transform.position, tamanoSensor, 0f);

        bool miJugadorEstaAca = false;

        // Revisamos cada cosa que toca el botón
        foreach (Collider2D col in colisiones)
        {
            if (col.CompareTag("Player") || col.gameObject.name.Contains("PlayerSho") || col.gameObject.name.Contains("PlayerSpe"))
            {
                PhotonView pv = col.GetComponent<PhotonView>();

                // CRUCIAL: Solo habilitamos el botón si el personaje que lo pisa es el TUYO (el local)
                if (pv != null && pv.IsMine)
                {
                    miJugadorEstaAca = true;
                    break;
                }
            }
        }

        // Si TU personaje está parado encima y MANTENÉS apretada la Q
        if (miJugadorEstaAca && Input.GetKey(KeyCode.Q))
        {
            if (!qEstaPresionada)
            {
                qEstaPresionada = true;
                if (puertaObjetivo != null)
                {
                    puertaObjetivo.ActualizarEstadoBoton(idBotonUnico, true);
                }
            }
        }
        // Si soltás la Q o te salís del botón, se cancela tu voto
        else
        {
            if (qEstaPresionada)
            {
                qEstaPresionada = false;
                if (puertaObjetivo != null)
                {
                    puertaObjetivo.ActualizarEstadoBoton(idBotonUnico, false);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, tamanoSensor);
    }
}