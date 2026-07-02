using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class WeaponStats : MonoBehaviourPun
{
    
    public int Damage { get; private set; }
    public float FireRate { get; private set; }
    public int MagazineSize { get; private set; }
    public float ReloadTime { get; private set; }
    public bool Automatic { get; private set; }
    public float Range { get; private set; }

    private void Awake()
    {
        ConfigureWeapon();
    }

    private void ConfigureWeapon()
    {
        PlayerAvatar avatar = PlayerInfo.GetAvatar(photonView.Owner);

        switch (avatar)
        {
            case PlayerAvatar.Specialist:

                Damage = 15;
                FireRate = 0.10f;
                MagazineSize = 30;
                ReloadTime = 2f;
                Automatic = true;
                Range = 20f;

                break;

            case PlayerAvatar.Shooter:

                Damage = 35;
                FireRate = 0.60f;
                MagazineSize = 8;
                ReloadTime = 2.5f;
                Automatic = false;
                Range = 25f;

                break;
        }
    }
}
