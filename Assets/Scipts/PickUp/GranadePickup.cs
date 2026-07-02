using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class GranadePickup : Pickup
{
    [SerializeField] private int amount = 1;

    protected override void Apply(GameObject player)
    {
        PlayerItems playerItems = player.GetComponent<PlayerItems>();

        if (playerItems != null)
        {
            playerItems.AddGrenades(amount);
        }
    }
}
