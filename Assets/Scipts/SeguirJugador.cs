using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class SeguirJugador : MonoBehaviourPun
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float velocidad = 2.5f;
    [SerializeField] private float distanciaDeteccion = 30f;

    [Header("Configuración de Combate")]
    [SerializeField] private int danioPorGolpe = 10;
    [SerializeField] private float tiempoEntreAtaques = 1.5f;
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (estaMuerto) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent(out PlayerManager playerHealth))
            {
                if (Time.time >= proximoAtaqueTime)
                {
                    proximoAtaqueTime = Time.time + tiempoEntreAtaques;
                    playerHealth.TakeDamage(danioPorGolpe);
                    Debug.Log($"El zombi atacó a {collision.gameObject.name}. Daño: {danioPorGolpe}");
                }
            }
        }
    }

    // --- SISTEMA DE DAÑO Y MUERTE EN RED ---

    // IMPORTANTE: Ahora recibe la cantidad de daño y el ID de quién disparó
    [PunRPC]
    public void RecibirDanioRed(int cantidadDanio, int idAtacante)
    {
        // Solo el Master Client maneja la vida global para que no haya desincronización
        if (!PhotonNetwork.IsMasterClient) return;
        if (estaMuerto) return;

        vidaActual -= cantidadDanio;
        Debug.Log($"Zombi dañado con {cantidadDanio}. Vida restante: {vidaActual}");

        if (vidaActual <= 0)
        {
            estaMuerto = true;
            rb.velocity = Vector2.zero;
            Debug.Log($"Zombi muerto. Procesando kill para el jugador con ID: {idAtacante}");

            // El Master Client busca al jugador que disparó usando su ID
            Player atacante = PhotonNetwork.CurrentRoom.GetPlayer(idAtacante);

            if (atacante != null)
            {
                // Le mandamos la orden de sumar baja SOLAMENTE a la PC del jugador que disparó
                photonView.RPC(nameof(RPC_OtorgarBaja), atacante);
            }

            PhotonNetwork.Destroy(gameObject);
        }
    }

    // Este RPC solo se va a ejecutar en la computadora del jugador que metió el último tiro
    [PunRPC]
    public void RPC_OtorgarBaja()
    {
        if (GestorScoreboard.Instancia != null)
        {
            GestorScoreboard.Instancia.RegistrarBaja();
        }
        else
        {
            Debug.LogError("No se encontró el GestorScoreboard en la escena.");
        }
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