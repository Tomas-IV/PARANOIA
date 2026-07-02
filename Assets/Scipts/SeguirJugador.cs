using UnityEngine;
using Photon.Pun; // <-- Clave para heredar de MonoBehaviourPun

public class SeguirJugador : MonoBehaviourPun
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float distanciaDeteccion = 30f;

    private Transform targetJugador;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        // 🌟 REGLA DE PHOTON: Solo el Master Client calcula el movimiento y la lógica de la IA
        if (!PhotonNetwork.IsMasterClient) return;

        // Buscamos cuál es el jugador en red más cercano en este momento
        BuscarObjetivoMasCercano();

        // Si no hay jugadores vivos o en la sala, nos quedamos quietos
        if (targetJugador == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float distancia = Vector2.Distance(transform.position, targetJugador.position);

        // Si está en rango de visión, avanzamos físicamente hacia él
        if (distancia <= distanciaDeteccion)
        {
            Vector2 direccion = (targetJugador.position - transform.position).normalized;
            rb.MovePosition(rb.position + direccion * velocidad * Time.fixedDeltaTime);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    // Algoritmo dinámico para detectar a los personajes instanciados por el PlayerSpawner
    private void BuscarObjetivoMasCercano()
    {
        // Buscamos a todos los personajes en el mapa usando la etiqueta "Player"
        GameObject[] jugadoresEnEscena = GameObject.FindGameObjectsWithTag("Player");

        if (jugadoresEnEscena.Length == 0)
        {
            targetJugador = null;
            return;
        }

        GameObject masCercano = null;
        float distanciaMinima = Mathf.Infinity;
        Vector3 posicionActual = transform.position;

        foreach (GameObject jugador in jugadoresEnEscena)
        {
            float distancia = Vector3.Distance(jugador.transform.position, posicionActual);
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                masCercano = jugador;
            }
        }

        // Asignamos el transform del personaje más cercano como el objetivo de caza
        if (masCercano != null)
        {
            targetJugador = masCercano.transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);
    }
}