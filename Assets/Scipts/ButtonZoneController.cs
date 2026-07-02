using UnityEngine;

public class ButtonZoneController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DoorController puertaObjetivo;

    [Header("Configuracion de Equipo")]
    [SerializeField] private int idBotonUnico;

    [Header("Configuracion del Sensor (Reemplaza al Trigger)")]
    [SerializeField] private Vector2 tamanoSensor = new Vector2(0.39f, 0.30f); // El tamaño que tenias en tu Trigger
    [SerializeField] private LayerMask capasJugador; // Configura esto en 'Default' o la capa de tus Players

    private bool jugadorEnZona = false;

    private void Update()
    {
        // Escaneamos activamente el area del boton cada frame
        // Esto evita que la fisica se duerma si el jugador se queda quieto
        Collider2D[] colisiones = Physics2D.OverlapBoxAll(transform.position, tamanoSensor, 0f);

        jugadorEnZona = false;

        foreach (var col in colisiones)
        {
            if (col.CompareTag("Player") ||
                col.gameObject.name.Contains("PlayerSho") ||
                col.gameObject.name.Contains("PlayerSpe"))
            {
                jugadorEnZona = true;
                break; // Si encontramos al menos uno, habilitamos la Q y cortamos el bucle
            }
        }

        // Si el escáner detectó un jugador y este presiona la Q, mandamos la señal
        if (jugadorEnZona && Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Boton " + idBotonUnico + " detecto la Q correctamente.");
            if (puertaObjetivo != null)
            {
                puertaObjetivo.EnviarConfirmacionInput(idBotonUnico);
            }
        }
    }

    // Dibujamos el sensor en el editor de Unity para que puedas ver el area exacta que editas
    private void OnDrawGizmosDefault()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, tamanoSensor);
    }
}