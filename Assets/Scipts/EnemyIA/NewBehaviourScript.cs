//using System;
//using UnityEngine;
//using System.Collections;
//using Unity.VisualScripting;
//using Photon.Pun; // <-- NUEVO: Librería de Photon

//public class EnemyController : MonoBehaviourPun // <-- CAMBIO: Hereda de MonoBehaviourPun
//{
//    public enum Mode
//    {
//        Seek,
//        Flee,
//        Arrive,
//        Pursue,
//        Evade,
//        Wander,
//        AttackRange
//    }

//    public bool playerInShieldRange = true;
//    [SerializeField] private Mode mode;
//    [SerializeField] private EnemyData data;
//    [SerializeField] private Transform player;
//    [SerializeField] private LineOfSight2D los;
//    [SerializeField] private Rigidbody2D playerRb;
//    ////public EnemyData Data => data;

//    private IEnemyMover mover;
//    private IEnemyAttacker attacker;
//    private IEnemyShooter shooter;
//    public bool IsInitialized { get; private set; }
//    private DecisionNode tree;
//    private EnemyContext context;

//    private StatusController statusController;
//    private bool isLocked = false;

//    public void SetLocked(bool value)
//    {
//        isLocked = value;
//    }

//    private void Awake()
//    {
//        mover = GetComponent<IEnemyMover>();
//        attacker = GetComponent<IEnemyAttacker>();
//        shooter = GetComponent<IEnemyShooter>();
//        statusController = GetComponent<StatusController>();
//    }

//    private IEnumerator Start()
//    {
//        // El Master Client se encarga de registrar el enemigo en el GameManager global
//        if (PhotonNetwork.IsMasterClient)
//        {
//            if (GameManager.Instance != null)
//            {
//                GameManager.Instance.AddEnemie(this.gameObject);
//            }
//        }

//        // Esperamos a que los personajes estén instanciados en la escena de red
//        while (GameManager.Instance == null || !TargetMasCercano())
//        {
//            yield return null;
//        }

//        tree = EnemyDecisionTree.CreateTree(data.attackRange);
//        IsInitialized = true;
//        mode = Mode.Wander;
//    }

//    private void FixedUpdate()
//    {
//        // ?? REGLA CRÍTICA DE PHOTON: Solo el Master Client ejecuta las decisiones y movimientos de la IA
//        if (!PhotonNetwork.IsMasterClient) return;

//        if (isLocked)
//        {
//            mover.Move(Vector2.zero);
//            return;
//        }

//        // Buscamos constantemente al personaje más cercano para redirigir la horda
//        if (!TargetMasCercano()) return;

//        tree.Evaluate(this, context);

//        Vector2 dir = Vector2.zero;
//        switch (mode)
//        {
//            case Mode.Seek:
//                dir = SteeringBehaviours2D.Seek(transform.position, player.position);
//                break;

//            case Mode.Flee:
//                dir = SteeringBehaviours2D.Flee(transform.position, player.position);
//                break;

//            case Mode.Arrive:
//                dir = SteeringBehaviours2D.Arrive(transform.position, player.position, 5f);
//                break;

//            case Mode.Pursue:
//                dir = SteeringBehaviours2D.Pursue(transform.position, player, playerRb, 0.5f);
//                break;

//            case Mode.Evade:
//                dir = SteeringBehaviours2D.Evade(transform.position, player, playerRb, 0.5f);
//                break;

//            case Mode.Wander:
//                dir = Vector2.zero;
//                break;

//            case Mode.AttackRange:
//                if (statusController != null && !statusController.CanAttack())
//                    return;

//                if (shooter != null)
//                {
//                    shooter.Shoot(player);
//                }
//                else if (attacker != null)
//                {
//                    attacker.Attack();
//                }

//                dir = Vector2.zero;
//                break;
//        }

//        if (mover == null)
//        {
//            Debug.LogError("Mover NO encontrado en " + gameObject.name);
//        }

//        mover.Move(dir);

//        if (data.Escudo == true)
//        {
//            EscudoRango();
//        }
//    }

//    // ?? NUEVO: Método para buscar al personaje más cercano en la partida de red
//    private bool TargetMasCercano()
//    {
//        // Buscamos todos los objetos que tengan el Tag "Player" en la partida
//        GameObject[] jugadores = GameObject.FindGameObjectsWithTag("Player");

//        if (jugadores.Length == 0) return false;

//        GameObject objetivoMasCercano = null;
//        float distanciaMinima = Mathf.Infinity;
//        Vector3 posicionActual = transform.position;

//        foreach (GameObject jugadorObj in jugadores)
//        {
//            float distancia = Vector3.Distance(jugadorObj.transform.position, posicionActual);
//            if (distancia < distanciaMinima)
//            {
//                distanciaMinima = distancia;
//                objetivoMasCercano = jugadorObj;
//            }
//        }

//        if (objetivoMasCercano != null)
//        {
//            player = objetivoMasCercano.transform;
//            playerRb = objetivoMasCercano.GetComponent<Rigidbody2D>();

//            // Actualizamos el contexto para el Árbol de Decisiones
//            if (context == null) context = new EnemyContext();
//            context.self = transform;
//            context.player = player;
//            context.los = los;

//            return true;
//        }

//        return false;
//    }

//    public void EscudoRango()
//    {
//        if (player == null) return;

//        float distance = Vector3.Distance(transform.position, player.position);
//        bool currentlyInRange = distance <= data.RangoEscudo;

//        if (!currentlyInRange && playerInShieldRange)
//        {
//            Debug.Log("El jugador salió del rango");
//        }

//        playerInShieldRange = currentlyInRange;
//    }

//    public EnemyContext GetContext()
//    {
//        return context;
//    }

//    public void SetMode(EnemyController.Mode newMode)
//    {
//        mode = newMode;
//    }

//    public void ApplyKnockback(Vector2 velocity, float duration)
//    {
//        // Los impactos físicos se aplican prioritariamente en el Master Client
//        if (PhotonNetwork.IsMasterClient)
//        {
//            mover.ApplyKnockback(velocity, duration);
//        }
//    }
//}