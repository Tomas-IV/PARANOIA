using UnityEngine;

public class SeguirJugador : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float distanciaDeteccion = 30f; // Lo agrandamos para asegurarnos de que lo vea

    private Transform targetJugador;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // Buscamos al jugador en la escena mediante su etiqueta (Tag)
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");

        if (jugadorObj != null)
        {
            targetJugador = jugadorObj.transform;
            // Mensaje en blanco: Todo viene bien
            Debug.Log("¡SÍ! El zombi encontró al jugador llamado: " + jugadorObj.name);
        }
        else
        {
            // Mensaje en rojo: Al personaje le falta el Tag 'Player'
            Debug.LogError("¡NO! El zombi no encuentra a nadie con el Tag 'Player' en la escena. Revisá el Inspector de tu personaje.");
        }
    }

    private void FixedUpdate()
    {
        // Si no encontró al jugador en el Start, el zombi no hace nada
        if (targetJugador == null) return;

        // Calculamos la distancia actual entre el zombi y el jugador
        float distancia = Vector2.Distance(transform.position, targetJugador.position);

        // Si el jugador está dentro de su rango de visión, lo persigue
        if (distancia <= distanciaDeteccion)
        {
            // Calculamos la dirección hacia el jugador
            Vector2 direccion = (targetJugador.position - transform.position).normalized;

            // Movemos al zombi usando el Rigidbody2D
            rb.MovePosition(rb.position + direccion * velocidad * Time.fixedDeltaTime);
        }
        else
        {
            // Si el jugador se aleja, el zombi se frena
            rb.velocity = Vector2.zero;
        }
    }

    // Dibuja el círculo rojo de visión en la pestaña Scene para control visual
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);
    }
}