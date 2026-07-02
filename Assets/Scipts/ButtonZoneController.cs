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
    [SerializeField] private LayerMask capasJugador;

    private bool jugadorEnZona = false;
    private bool qEstaPresionada = false; // Registra si actualmente la estas manteniendo

    private void Update()
    {
        Collider2D[] colisiones = Physics2D.OverlapBoxAll(transform.position, tamanoSensor, 0f, capasJugador);
        jugadorEnZona = colisiones.Length > 0;

        // Si estas en la zona y MANTIENES APRETADA la Q
        if (jugadorEnZona && Input.GetKey(KeyCode.Q))
        {
            if (!qEstaPresionada)
            {
                qEstaPresionada = true; // Empieza a mantener
                if (puertaObjetivo != null)
                {
                    puertaObjetivo.ActualizarEstadoBoton(idBotonUnico, true);
                }
            }
        }
        // Si sueltas la Q o te sales de la zona, SE CANCELA
        else
        {
            if (qEstaPresionada)
            {
                qEstaPresionada = false; // Dejo de mantener
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