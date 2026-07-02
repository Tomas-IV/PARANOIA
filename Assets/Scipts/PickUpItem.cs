using System.Collections.Generic;
using System.Threading.Tasks;
using Photon.Pun;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.RemoteConfig;
using UnityEngine;

public class PickUpItem : MonoBehaviour
{
    [System.Serializable]
    public class PickupEntry
    {
        public string id;
        public GameObject prefab;
    }

    [Header("Pickups")]
    [SerializeField] private PickupEntry[] pickups;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    private bool spawnEnabled = true;
    private int amountToSpawn = 1;
    private string enabledPickups = "";

    private readonly List<GameObject> availablePickups = new();

    private async void Start()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        await InitializeRemoteConfig();

        if (!spawnEnabled)
        {
            Debug.Log("Pickup spawn disabled.");
            return;
        }

        BuildPickupList();

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

        spawnEnabled = RemoteConfigService.Instance.appConfig.GetBool(
            "pickup_spawn_enabled",
            true);

        amountToSpawn = RemoteConfigService.Instance.appConfig.GetInt(
            "pickup_spawn_amount",
            1);

        enabledPickups = RemoteConfigService.Instance.appConfig.GetString(
            "enabled_pickups",
            "heal");

        Debug.Log("Remote Config Loaded");
        Debug.Log("Spawn Enabled: " + spawnEnabled);
        Debug.Log("Spawn Amount: " + amountToSpawn);
        Debug.Log("Enabled Pickups: " + enabledPickups);
    }

    private void BuildPickupList()
    {
        availablePickups.Clear();

        string[] ids = enabledPickups.Split(',');

        foreach (string id in ids)
        {
            string trimmed = id.Trim().ToLower();

            foreach (PickupEntry entry in pickups)
            {
                if (entry.id.ToLower() == trimmed)
                {
                    availablePickups.Add(entry.prefab);
                }
            }
        }
    }

    private void SpawnItems()
    {
        if (availablePickups.Count == 0)
        {
            Debug.LogWarning("No enabled pickups found.");
            return;
        }

        List<Transform> freePoints = new(spawnPoints);

        int spawnCount = Mathf.Min(amountToSpawn, freePoints.Count);

        for (int i = 0; i < spawnCount; i++)
        {
            int pointIndex = Random.Range(0, freePoints.Count);

            int pickupIndex = Random.Range(0, availablePickups.Count);

            PhotonNetwork.InstantiateRoomObject(
                availablePickups[pickupIndex].name,
                freePoints[pointIndex].position,
                Quaternion.identity);

            freePoints.RemoveAt(pointIndex);
        }
    }

    public struct UserAttributes { }
    public struct AppAttributes { }
}