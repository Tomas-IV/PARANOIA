using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class PlayerNickname : MonoBehaviourPun
{
    [SerializeField] private TMP_Text nicknameText;

    private void Start()
    {
        nicknameText.text = photonView.Owner.NickName;
    }
}
