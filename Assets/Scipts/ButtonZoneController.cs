using UnityEngine;
using Photon.Pun;

public class ButtonZoneController : MonoBehaviour
{
    [Header("Conexión")]
    public DoorController puertaPrincipal;
    public int idBoton;
    public float radioDeteccion = 0.6f;

    [Header("Configuración del Botón")]
    [Tooltip("Cuánto tiempo se mantiene apretado el botón con un solo toque")]
    public float tiempoActivo = 1.5f;

    private bool estadoAnterior = false;
    private float temporizador = 0f;

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

        // Cambio 1: Usamos GetKeyDown (se activa una sola vez al hundir la tecla)
        if (hayJugadorMio && Input.GetKeyDown(KeyCode.Q))
        {
            // Llenamos el temporizador con los segundos que elegimos
            temporizador = tiempoActivo;
            Debug.Log($"[Botón {idBoton}] ¡Q presionada! Esperando al compañero por {tiempoActivo} segundos...");
        }

        // Cambio 2: Si el temporizador tiene tiempo, lo vamos restando
        if (temporizador > 0)
        {
            temporizador -= Time.deltaTime;
        }

        // Cambio 3: El botón está "presionado" (true) mientras el temporizador sea mayor a cero
        bool estadoActual = temporizador > 0;

        // Si el estado cambió (pasó de apagado a prendido, o se le acabó el tiempo y se apagó)
        if (estadoActual != estadoAnterior)
        {
            estadoAnterior = estadoActual;

            if (puertaPrincipal != null)
            {
                puertaPrincipal.EnviarVoto(idBoton, estadoActual);
                Debug.Log($"[Botón {idBoton}] Estado enviado a la puerta: {estadoActual}");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}