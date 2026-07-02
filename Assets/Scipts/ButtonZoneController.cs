using UnityEngine;
using Photon.Pun;

public class ButtonZoneController : MonoBehaviourPun
{
    [Header("Referencias")]
    [SerializeField] private DoorController puertaObjetivo; // Arrastra la puerta aca en el Inspector

    private bool jugadorEnZona = false;

    private void Update()
    {
        // Cada cliente verifica de manera local si SU jugador esta en la zona y presiona ENTER
        if (jugadorEnZona && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            if (puertaObjetivo != null)
            {
                // Le avisamos a la puerta que este jugador dio el OK
                puertaObjetivo.EnviarConfirmacionInput();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detectamos si el objeto que entro es un Player
        if (collision.CompareTag("Player"))
        {
            PhotonView pv = collision.GetComponent<PhotonView>();

            // CRUCIAL: Solo nos importa si el personaje que entro es el NUESTRO (el local)
            if (pv != null && pv.IsMine)
            {
                jugadorEnZona = true;
                Debug.Log("Entraste al boton. Presiona ENTER para activar.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PhotonView pv = collision.GetComponent<PhotonView>();

            // Si el personaje que salio es el nuestro, deshabilitamos el Input
            if (pv != null && pv.IsMine)
            {
                jugadorEnZona = false;
                Debug.Log("Saliste del boton.");
            }
        }
    }
}