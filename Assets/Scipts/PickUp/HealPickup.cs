using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


public class HealPickup : Pickup
{
    [SerializeField] int healAmount = 25;

    protected override void Apply(GameObject player)
    {
        player.GetComponent<PlayerManager>().Heal(healAmount);
    }
}
