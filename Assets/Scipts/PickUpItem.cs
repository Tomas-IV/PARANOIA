using System.Collections.Generic;
using System.Threading.Tasks;
using Photon.Pun;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private bool spawnEnabled = true;
    private int amountToSpawn = 1;

    private async void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        await InitializeRemoteConfig();

        if (!spawnEnabled)
        {
            Debug.Log("Pickup spawn deshabilitado desde Remote Config.");
            return;
        }

        SpawnItems();
    }

    private async Task InitializeRemoteConfig()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        await RemoteConfigService.Instance.FetchConfigsAsync(
            new UserAttributes(),
            new AppAttributes());

        spawnEnabled = RemoteConfigService.Instance.appConfig.GetBool("pickup_spawn_enabled",true);

        amountToSpawn = RemoteConfigService.Instance.appConfig.GetInt("pickup_spawn_amount",1);

        Debug.Log("Remote Config Loaded:");
        Debug.Log("spawnEnabled: " + spawnEnabled);
        Debug.Log("amountToSpawn: " + amountToSpawn);
    }

    private void SpawnItems()
    {
        if (spawnPoints.Length == 0)
        {
            Debug.LogWarning("No hay SpawnPoints configurados.");
            return;
        }

        List<Transform> availablePoints = new List<Transform>(spawnPoints);

        int spawnCount = Mathf.Min(amountToSpawn, availablePoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int randomIndex = Random.Range(0, availablePoints.Count);

            PhotonNetwork.InstantiateRoomObject(itemPrefab.name,availablePoints[randomIndex].position,Quaternion.identity);

            availablePoints.RemoveAt(randomIndex);
        }
    }

    public struct UserAttributes { }
    public struct AppAttributes { }
}