using UnityEngine;
using Photon.Pun;

public class SeguirJugador : MonoBehaviourPun
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float velocidad = 2.5f;
    [SerializeField] private float distanciaDeteccion = 30f;

    [Header("Configuración de Combate")]
    [SerializeField] private int danioPorGolpe = 10; // Cuánta vida le saca al jugador
    [SerializeField] private float tiempoEntreAtaques = 1.5f; // Segundos de espera para volver a pegar
    private float proximoAtaqueTime;

    [Header("Configuración de Vida")]
    [SerializeField] private int vidaMax = 105;
    private int vidaActual;
    private bool estaMuerto = false;

    private Transform targetJugador;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        vidaActual = vidaMax;
        estaMuerto = false;
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (estaMuerto) return;
        if (!PhotonNetwork.IsMasterClient) return;

        BuscarObjetivoMasCercano();

        if (targetJugador == null)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float distancia = Vector2.Distance(transform.position, targetJugador.position);

        if (distancia <= distanciaDeteccion)
        {
            Vector2 direccion = (targetJugador.position - transform.position).normalized;
            rb.MovePosition(rb.position + direccion * velocidad * Time.fixedDeltaTime);

            float anguloZ = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            rb.MoveRotation(anguloZ);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    //  NUEVO: Detección física cuando el zombi toca al Player
    private void OnCollisionStay2D(Collision2D collision)
    {
        // Solo el Master Client gestiona el daño de las IAs en red
        if (!PhotonNetwork.IsMasterClient) return;
        if (estaMuerto) return;

        // Comprobamos si el objeto tocado es un Jugador
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent(out PlayerManager playerHealth))
            {
                // Controlamos el temporizador para que no pegue en cada frame
                if (Time.time >= proximoAtaqueTime)
                {
                    proximoAtaqueTime = Time.time + tiempoEntreAtaques;

                    // Le infligimos daño usando el método nativo que me pasaste
                    playerHealth.TakeDamage(danioPorGolpe);
                    Debug.Log($"El zombi atacó a {collision.gameObject.name}. Daño: {danioPorGolpe}");
                }
            }
        }
    }

    public void RecibirDanio(int cantidadDanio)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (estaMuerto) return;

        vidaActual -= cantidadDanio;
        Debug.Log($"Zombi dañado con {cantidadDanio}. Vida restante: {vidaActual}");

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private void Morir()
    {
        estaMuerto = true;
        rb.velocity = Vector2.zero;
        Debug.Log("Zombi muerto. Desactivando y mandando al pool...");
        PhotonNetwork.Destroy(gameObject);
    }

    private void BuscarObjetivoMasCercano()
    {
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
            // Solo perseguir si el jugador está vivo (tu enum PlayerLifeState)
            if (jugador.TryGetComponent(out PlayerManager manager) && manager.IsDowned())
                continue;

            float distancia = Vector3.Distance(jugador.transform.position, posicionActual);
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                masCercano = jugador;
            }
        }

        if (masCercano != null)
        {
            targetJugador = masCercano.transform;
        }
        else
        {
            targetJugador = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);
    }
}