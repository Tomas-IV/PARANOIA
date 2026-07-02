using UnityEngine;
using Photon.Pun;

public class ButtonZoneController : MonoBehaviour
{
    [Header("Configuracion de Equipo")]
    public int idBoton;
    public float radioDeteccion = 0.6f;

    private DoorController[] puertasEnEscena;
    private GameObject jugadorLocal = null;
    private bool presionandoQ = false;

    void Start()
    {
        // Modo automático: busca todas las puertas del mapa sin necesidad de arrastrar nada al Inspector
        puertasEnEscena = FindObjectsOfType<DoorController>();
    }

    void Update()
    {
        if (jugadorLocal == null)
        {
            BuscarMiJugadorLocal();
        }

        bool hayJugadorMio = false;

        if (jugadorLocal != null)
        {
            float distancia = Vector2.Distance(transform.position, jugadorLocal.transform.position);
            if (distancia <= radioDeteccion)
            {
                hayJugadorMio = true;
            }
        }

        bool estadoActual = (hayJugadorMio && Input.GetKey(KeyCode.Q));

        if (estadoActual != presionandoQ)
        {
            presionandoQ = estadoActual;

            // Le avisamos a todas las puertas que detectamos en la escena en simultáneo
            foreach (DoorController puerta in puertasEnEscena)
            {
                if (puerta != null)
                {
                    puerta.EnviarVoto(idBoton, presionandoQ);
                }
            }
        }
    }

    private void BuscarMiJugadorLocal()
    {
        PhotonView[] todosLosViews = FindObjectsOfType<PhotonView>();
        foreach (PhotonView view in todosLosViews)
        {
            if (view.IsMine && (view.CompareTag("Player") ||
                                view.gameObject.name.Contains("PlayerSho") ||
                                view.gameObject.name.Contains("PlayerSpe")))
            {
                jugadorLocal = view.gameObject;
                return;
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}