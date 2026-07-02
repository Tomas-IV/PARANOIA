using UnityEngine;
using Photon.Pun;

public class ButtonZoneController : MonoBehaviourPun
{
    [Header("Referencias")]
    [SerializeField] private DoorController puertaObjetivo;

    [Header("Configuracion de Equipo")]
    [SerializeField, Tooltip("Asignar 1 o 2: cada botón debe tener un id distinto (1 y 2).")]
    private int idBotonUnico = 1;

    [Header("Configuracion del Sensor")]
    [SerializeField] private Vector2 tamanoSensor = new Vector2(0.39f, 0.30f);

    private bool qEstaPresionada = false;

    private void Awake()
    {
        if (puertaObjetivo == null)
        {
            Debug.LogWarning($"{name}: 'puertaObjetivo' no asignada. Asigna la misma instancia de DoorController en ambos botones.");
        }
    }

    private void OnValidate()
    {
        // Forzar id válido (1 o 2) en el inspector
        if (idBotonUnico < 1) idBotonUnico = 1;
        if (idBotonUnico > 2) idBotonUnico = 2;
    }

    private void Update()
    {
        Collider2D[] colisiones = Physics2D.OverlapBoxAll(transform.position, tamanoSensor, 0f);
        bool miJugadorEstaAca = false;

        foreach (Collider2D col in colisiones)
        {
            if (col.CompareTag("Player") || col.gameObject.name.Contains("PlayerSho") || col.gameObject.name.Contains("PlayerSpe"))
            {
                PhotonView pv = col.GetComponent<PhotonView>();
                if (pv == null)
                {
                    pv = col.GetComponentInParent<PhotonView>();
                }

                if (pv != null && pv.IsMine)
                {
                    miJugadorEstaAca = true;
                    break;
                }
            }
        }

        if (miJugadorEstaAca && Input.GetKey(KeyCode.Q))
        {
            if (!qEstaPresionada)
            {
                qEstaPresionada = true;
                if (puertaObjetivo != null) puertaObjetivo.ActualizarEstadoBoton(idBotonUnico, true);
            }
        }
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