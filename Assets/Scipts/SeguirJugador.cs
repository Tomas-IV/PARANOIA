using UnityEngine;
using Photon.Pun;

public class SeguirJugador : MonoBehaviourPun
{
    [Header("Configuración de Movimiento")]
    [SerializeField] private float velocidad = 2.5f;
    [SerializeField] private float distanciaDeteccion = 30f;

    [Header("Configuración de Vida")]
    [SerializeField] private int vidaMax = 105; // 105 hp hace que el Shooter (35 dmg) lo mate de 3 tiros y el Specialist (15 dmg) de 7
    private int vidaActual;
    private bool estaMuerto = false;

    private Transform targetJugador;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        vidaActual = vidaMax;
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

    // FUNCIÓN CLAVE: Recibe el daño exacto enviado desde el Raycast del arma
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
        Debug.Log("Zombi muerto. Eliminando de la red...");
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
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccion);
    }
}