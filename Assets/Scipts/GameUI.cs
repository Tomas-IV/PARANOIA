using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class GameUI : MonoBehaviourPunCallbacks
{
    /*public TextMeshProUGUI notice;
    public TextMeshProUGUI roleText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI stateText;

    private int role = -1;
    private PlayerController localPlayer;

    void Update()
    {
        FindLocalPlayer();
        UpdateRole();

        if (GameManager.Instance == null) return;

        UpdateNotice();
        UpdateTimer();
        UpdateStateText();
    }

    void FindLocalPlayer()
    {
        if (localPlayer != null) return;

        PlayerController[] players = FindObjectsOfType<PlayerController>();

        foreach (var p in players)
        {
            if (p.photonView.IsMine)
            {
                localPlayer = p;
                break;
            }
        }
    }

    //  role (siempre chequea sync)
    void UpdateRole()
    {
        if (PhotonNetwork.LocalPlayer.CustomProperties == null) return;

        if (PhotonNetwork.LocalPlayer.CustomProperties.ContainsKey("role"))
        {
            role = (int)PhotonNetwork.LocalPlayer.CustomProperties["role"];
            roleText.gameObject.SetActive(true);
            roleText.text = role == 1 ? "Seeker" : "Hider";
        }
    }

    //  gamestate
    void UpdateNotice()
    {
        switch (GameManager.Instance.currentState)
        {
            case GameManager.GameState.Waiting:
                notice.gameObject.SetActive(true);
                notice.text = "Waiting...";
                break;

            case GameManager.GameState.Hiding:
                notice.gameObject.SetActive(true);
                notice.text = (role == 1) ? "Wait..." : "Hide!";
                break;

            case GameManager.GameState.Searching:
                notice.gameObject.SetActive(false);
                break;

            case GameManager.GameState.End:
                notice.gameObject.SetActive(true);

                if (GameManager.Instance.gameResult == 1)
                    notice.text = (role == 1) ? "YOU WIN" : "YOU LOSE";
                else
                    notice.text = (role == 0) ? "YOU WIN" : "YOU LOSE";
                break;
        }
    }

    // timer
    void UpdateTimer()
    {
        float t = GameManager.Instance.GetRemainingTime();

        if (GameManager.Instance.currentState == GameManager.GameState.Hiding)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = "Hide: " + Mathf.CeilToInt(t);
        }
        else if (GameManager.Instance.currentState == GameManager.GameState.Searching)
        {
            timerText.gameObject.SetActive(true);
            timerText.text = "Time: " + Mathf.CeilToInt(t);
        }
        else
        {
            timerText.gameObject.SetActive(false);
        }
    }

    //  freeze / revive
    void UpdateStateText()
    {
        stateText.gameObject.SetActive(false);

        if (localPlayer == null) return;

        // frozen ui
        if (localPlayer.IsCaptured)
        {
            float timeLeft = localPlayer.GetRemainingReviveTime();

            stateText.gameObject.SetActive(true);
            string reviveText = "Waiting for revive";
            if (localPlayer.IsBeingRevived) reviveText = "Reviving in: " + timeLeft.ToString("F1") + "s";
            stateText.text =
                "YOU ARE FROZEN!!\n" +
                reviveText;

            return;
        }

        // reviviendo
        if (role == 0 && Input.GetKey(KeyCode.E))
        {
            PlayerController target = GetClosestFrozen();

            stateText.gameObject.SetActive(true);

            if (target != null)
            {
                float timeLeft = target.GetRemainingReviveTime();

                stateText.text =
                    "REVIVING ALLY...\n" +
                    timeLeft.ToString("F1") + "s";
                return;
            }
            
        }
    }

    PlayerController GetClosestFrozen()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();

        PlayerController closest = null;
        float minDist = Mathf.Infinity;

        foreach (var p in players)
        {
            if (!p.IsCaptured) continue;

            float dist = Vector2.Distance(transform.position, p.transform.position);

            if (dist < minDist)
            {
                minDist = dist;
                closest = p;
            }
        }

        return closest;
    }

    */
}
