using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class EnemyPoolManager : MonoBehaviour, IPunPrefabPool
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject enemyPrefab; // Arrastrá el prefab del zombi acá
    [SerializeField] private int poolInitialSize = 10; // Cantidad de zombis precargados ocultos

    private Queue<GameObject> pooledObjects = new Queue<GameObject>();

    private void Start()
    {
        // Registramos este script en Photon como el gestor oficial de instanciación
        PhotonNetwork.PrefabPool = this;

        if (enemyPrefab != null)
        {
            PrewarmPool();
        }
        else
        {
            Debug.LogError("¡No asignaste el Prefab del Zombi en el EnemyPoolManager!");
        }
    }

    // Instancia los enemigos iniciales apagados para ganar rendimiento
    private void PrewarmPool()
    {
        for (int i = 0; i < poolInitialSize; i++)
        {
            GameObject obj = Instantiate(enemyPrefab);
            obj.name = enemyPrefab.name; // Photon necesita que el nombre coincida exacto
            obj.SetActive(false);
            pooledObjects.Enqueue(obj);
        }
    }

    // INTERCEPCIÓN DE PHOTON NETWORK INSTANTIATE
    public GameObject Instantiate(string prefabId, Vector3 position, Quaternion rotation)
    {
        // Si el prefab solicitado coincide con nuestro enemigo controlado por el pool
        if (prefabId == enemyPrefab.name)
        {
            GameObject obj;

            if (pooledObjects.Count > 0)
            {
                // Sacamos uno existente del pool
                obj = pooledObjects.Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
                obj.SetActive(true);
            }
            else
            {
                // Si la horda supera el tamaño inicial, creamos uno nuevo dinámicamente
                obj = Instantiate(enemyPrefab, position, rotation);
                obj.name = enemyPrefab.name;
            }

            return obj;
        }

        // Si Photon pide spawnear otra cosa (ej: las balas o jugadores), lo maneja por defecto
        return Instantiate(Resources.Load<GameObject>(prefabId), position, rotation);
    }

    // INTERCEPCIÓN DE PHOTON NETWORK DESTROY
    public void Destroy(GameObject gameObject)
    {
        // Si es uno de nuestros zombis, lo apagamos y lo devolvemos a la fila del pool
        if (gameObject.name == enemyPrefab.name)
        {
            gameObject.SetActive(false);
            pooledObjects.Enqueue(gameObject);
        }
        else
        {
            // Si es otro objeto ajeno al pool, lo destruye de verdad
            Destroy(gameObject);
        }
    }
}