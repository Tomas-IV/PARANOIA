using System.Collections;
using UnityEngine;
using Photon.Pun;

public class EnemySpawn : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    [SerializeField] private GameObject[] enemyPrefabs;

    [Header("Spawn Positions")]
    [SerializeField] private Transform[] spawnPositions;

    [Header("Configuración de Oleada")]
    [SerializeField] private float tiempoEntreZombies = 3f;
    [SerializeField] private int cantidadMaxima = 10;

    private int zombiesCreados = 0;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (zombiesCreados < cantidadMaxima)
        {
            yield return new WaitForSeconds(tiempoEntreZombies);

            if (enemyPrefabs.Length > 0 && spawnPositions.Length > 0)
            {
                int indicePunto = Random.Range(0, spawnPositions.Length);
                Vector3 posicionSpawn = spawnPositions[indicePunto].position;

                int indiceEnemigo = Random.Range(0, enemyPrefabs.Length);
                GameObject enemigoElegido = enemyPrefabs[indiceEnemigo];

                // --- GENERACIÓN EN RED (PHOTON) ---
                if (PhotonNetwork.IsConnected)
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        PhotonNetwork.Instantiate(enemigoElegido.name, posicionSpawn, Quaternion.identity);
                        zombiesCreados++;
                    }
                }
                // --- GENERACIÓN LOCAL ---
                else
                {
                    Instantiate(enemigoElegido, posicionSpawn, Quaternion.identity);
                    zombiesCreados++;
                }
            }
            else
            {
                Debug.LogWarning("¡Faltan asignar elementos en Enemy Prefabs o Spawn Positions del Inspector!");
                yield break;
            }
        }
    }
}