using UnityEngine;

public class ButtonZoneController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private DoorController puertaObjetivo;

    [Header("Configuracion de Equipo")]
    [SerializeField] private int idBotonUnico;

    private bool jugadorEnZona = false;

    private void Update()
    {
        // Si cualquiera de tus personajes esta en la zona y presiona la Q
        if (jugadorEnZona && Input.GetKeyDown(KeyCode.Q))
        {
            if (puertaObjetivo != null)
            {
                puertaObjetivo.EnviarConfirmacionInput(idBotonUnico);
            }
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Validamos por tag "Player" o si el objeto se llama como tus personajes seleccionables
        if (collision.CompareTag("Player") ||
            collision.gameObject.name.Contains("PlayerSho") ||
            collision.gameObject.name.Contains("PlayerSpe"))
        {
            jugadorEnZona = true;
        }
    }
    //
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") ||
            collision.gameObject.name.Contains("PlayerSho") ||
            collision.gameObject.name.Contains("PlayerSpe"))
        {
            jugadorEnZona = false;
            Debug.Log("Saliste del boton " + idBotonUnico);
        }
    }
}