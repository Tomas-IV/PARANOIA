using UnityEngine;

public class ButtonZoneController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DoorController puertaObjetivo;

    [Header("Configuracion de Equipo")]
    [SerializeField] private int idBotonUnico;

    [Header("Configuracion del Sensor")]
    [SerializeField] private Vector2 tamanoSensor = new Vector2(0.39f, 0.30f);
    [SerializeField] private LayerMask capasJugador;

    private bool jugadorEnZona = false;

    private void Update()
    {
        Collider2D[] colisiones = Physics2D.OverlapBoxAll(transform.position, tamanoSensor, 0f, capasJugador);

        jugadorEnZona = colisiones.Length > 0;

        if (jugadorEnZona && Input.GetKeyDown(KeyCode.Q))
        {
            if (puertaObjetivo != null)
            {
                Debug.Log($"[BOTON] Enviando señal de boton {idBotonUnico}");
                puertaObjetivo.EnviarConfirmacionInput(idBotonUnico);
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, tamanoSensor);
    }
}