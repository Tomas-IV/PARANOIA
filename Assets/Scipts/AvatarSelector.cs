using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSelector : MonoBehaviour
{
    public void SelectSpecialist()
    {
        PlayerPrefs.SetInt("Avatar", (int)PlayerAvatar.Specialist);
        PlayerPrefs.Save();
        PlayerInfo.SetAvatar(PlayerAvatar.Specialist);
        Debug.Log("Avatar seleccionado: Especialista");
    }
    
    public void SelectShooter()
    {
        PlayerPrefs.SetInt("Avatar", (int)PlayerAvatar.Shooter);
        PlayerPrefs.Save();
        PlayerInfo.SetAvatar(PlayerAvatar.Shooter);
        Debug.Log("Avatar seleccionado: Tirador");
    }

    private void Start()
    {
        if (PlayerPrefs.HasKey("Avatar"))
        {
            PlayerAvatar avatar =
                (PlayerAvatar)PlayerPrefs.GetInt("Avatar");

            PlayerInfo.SetAvatar(avatar);
        }
    }
}
