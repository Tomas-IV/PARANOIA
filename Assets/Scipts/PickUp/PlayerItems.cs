using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItems : MonoBehaviour
{
    [Header("Grenades")]
    [SerializeField] private int maxGrenades = 3;

    public int GrenadeCount { get; private set; }

    private void Awake()
    {
        GrenadeCount = 0;
    }

    public bool AddGrenades(int amount)
    {
        if (amount <= 0)
            return false;

        int previousAmount = GrenadeCount;

        GrenadeCount = Mathf.Clamp(
            GrenadeCount + amount,
            0,
            maxGrenades);

        return GrenadeCount > previousAmount;
    }

    public bool ConsumeGrenade()
    {
        if (GrenadeCount <= 0)
            return false;

        GrenadeCount--;

        return true;
    }

    public bool HasGrenades()
    {
        return GrenadeCount > 0;
    }

    public int GetGrenadeCount()
    {
        return GrenadeCount;
    }
}
