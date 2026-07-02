using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyPoolManager : MonoBehaviour, IPunPrefabPool
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject enemyPrefab; // El prefab de tu zombi
    [SerializeField] private int poolInitialSize = 10;

    private Queue<GameObject> pooledObjects = new Queue<GameObject>();

    // Guardamos una referencia al pool por defecto de Photon para no romper los otros objetos
    private IPunPrefabPool defaultPhotonPool;

    private void Awake()
    {
        //  Guardamos el comportamiento original de Photon ANTES de sobreescribirlo
        defaultPhotonPool = PhotonNetwork.PrefabPool;
        PhotonNetwork.PrefabPool = this;
    }

    private void Start()
    {
        if (enemyPrefab != null)
        {
            //  REGLA CRUCIAL: El prewarm SOLO lo hace el MasterClient o rompe la red de los clientes
            if (PhotonNetwork.IsMasterClient || !PhotonNetwork.IsConnected)
            {
                PrewarmPool();
            }
        }
        else
        {
            Debug.LogError("¡No asignaste el Prefab del Zombi en el EnemyPoolManager!");
        }
    }

    private void PrewarmPool()
    {
        for (int i = 0; i < poolInitialSize; i++)
        {
            GameObject obj = Instantiate(enemyPrefab);
            obj.name = enemyPrefab.name;
            obj.SetActive(false);
            pooledObjects.Enqueue(obj);
        }
    }

    // INTERCEPCIÓN DE PHOTON NETWORK INSTANTIATE
    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        // Si lo que se va a instanciar es nuestro Zombi, usamos nuestro reciclaje
        if (enemyPrefab != null && prefabId == enemyPrefab.name)
        {
            GameObject obj;

            if (pooledObjects.Count > 0)
            {
                obj = pooledObjects.Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
            }
            else
            {
                obj = Instantiate(enemyPrefab, position, rotation);
                obj.name = enemyPrefab.name;
            }

            return obj;
        }

        //  CORRECCIÓN HISTÓRICA: Si Photon pide spawnear balas, efectos o jugadores, 
        // se lo devolvemos a Photon para que lo maneje de forma nativa sin romper nada.
        return defaultPhotonPool.Instantiate(prefabId, position, rotation);
    }

    // INTERCEPCIÓN DE PHOTON NETWORK DESTROY
    public void Destroy(GameObject gameObject)
    {
        // Si el objeto que se muere es nuestro zombi, lo guardamos apagado
        if (enemyPrefab != null && gameObject.name == enemyPrefab.name)
        {
            gameObject.SetActive(false);
            pooledObjects.Enqueue(gameObject);
        }
        else
        {
            //  Si es una bala, jugador o VFX, dejamos que Photon lo destruya en red normalmente
            defaultPhotonPool.Destroy(gameObject);
        }
    }
}