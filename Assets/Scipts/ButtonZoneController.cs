using UnityEngine;
using Photon.Pun;

public class ButtonZoneController : MonoBehaviourPun
{
    [Header("Referencias")]
    [SerializeField] private DoorController puertaObjetivo;

    private bool jugadorEnZona = false;

    private void Update()
    {
        if (jugadorEnZona)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (puertaObjetivo != null)
                {
                    puertaObjetivo.EnviarConfirmacionInput();
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PhotonView pv = collision.GetComponent<PhotonView>();

            if (pv != null)
            {
                if (pv.IsMine)
                {
                    jugadorEnZona = true;
                    Debug.Log("Entraste al boton. Presiona Q para activar.");
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PhotonView pv = collision.GetComponent<PhotonView>();

            if (pv != null)
            {
                if (pv.IsMine)
                {
                    jugadorEnZona = false;
                    Debug.Log("Saliste del boton.");
                }
            }
        }
    }
}