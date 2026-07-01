using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;


public enum PlayerAvatar
{
    Specialist = 0,
    Shooter = 1,
}

public static class PlayerInfo
{
    public const string AvatarKey = "Avatar";

    public static void SetAvatar(PlayerAvatar avatar)
    {
        Hashtable props = new Hashtable
        {
            { AvatarKey, (int)avatar }
        };

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    public static PlayerAvatar GetAvatar(Player player)
    {
        if (player.CustomProperties.TryGetValue(AvatarKey, out object value))
        {
            return (PlayerAvatar)(int)value;
        }

        return PlayerAvatar.Specialist;
    }
}
