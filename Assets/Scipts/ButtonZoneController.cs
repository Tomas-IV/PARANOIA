using UnityEngine;
using Photon.Pun;

public class ButtonZoneController : MonoBehaviour
{
    [Header("Conexión Principal")]
    [SerializeField] private DoorController puertaObjetivo;
    [SerializeField] private int idBotonUnico;

    [Header("Ajuste de Margen")]
    [SerializeField] private float radioDeteccion = 0.5f; // Qué tan cerca hay que estar del centro del botón

    private GameObject jugadorLocal = null;
    private bool estoyPresionandoQ = false;

    private void Update()
    {
        // Si aún no encontramos nuestro personaje local en esta PC, lo buscamos
        if (jugadorLocal == null)
        {
            BuscarMiJugadorLocal();
        }

        bool cercaDelBoton = false;

        if (jugadorLocal != null)
        {
            // MODO NUEVO: Cálculo matemático de distancia pura. Saltea bugs de colisiones.
            float distancia = Vector2.Distance(transform.position, jugadorLocal.transform.position);
            if (distancia <= radioDeteccion)
            {
                cercaDelBoton = true;
            }
        }

        // Condición de interacción activa
        bool presionandoAhora = cercaDelBoton && Input.GetKey(KeyCode.Q);

        // Enviamos el paquete por red SOLO si cambió de estado (evita saturar Photon)
        if (presionandoAhora != estoyPresionandoQ)
        {
            estoyPresionandoQ = presionandoAhora;

            if (puertaObjetivo != null)
            {
                puertaObjetivo.NotificarEstadoBoton(idBotonUnico, estoyPresionandoQ);
            }
        }
    }

    private void BuscarMiJugadorLocal()
    {
        // Escaneamos la escena buscando el objeto que te pertenece a VOS en esta pantalla
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
        // Dibuja una esfera celeste en la pestaña Scene para ver el rango real del botón
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDeteccion);
    }
}