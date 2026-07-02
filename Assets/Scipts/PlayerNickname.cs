
using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PlayerNickname : MonoBehaviourPunCallbacks
{
    [SerializeField] private TMP_Text nicknameText;

    [Header("Team Colors")]
    [SerializeField] private Color team0Color = Color.blue;
    [SerializeField] private Color team1Color = Color.red;

    private int team = -1;

    private void Start()
    {
        nicknameText.text = photonView.Owner.NickName;

        // --- CONEXIÓN CON SAVEMANAGER ---
        // Chequeamos si es nuestro propio jugador para no sobreescribir nuestro save 
        // con el nombre de los demás conectados a la sala.
        if (photonView.IsMine && SaveManager.Instancia != null)
        {
            SaveManager.Instancia.datosDelJuego.ultimoNickname = PhotonNetwork.LocalPlayer.NickName;
        }

        TrySetTeamColor();
    }

    private void TrySetTeamColor()
    {
        if (photonView.Owner.CustomProperties != null &&
            photonView.Owner.CustomProperties.TryGetValue("team", out object t))
        {
            team = (int)t;
            ApplyColor();
        }
        else
        {
            Invoke(nameof(TrySetTeamColor), 0.5f);
        }
    }

    private void ApplyColor()
    {
        nicknameText.color = (team == 0) ? team0Color : team1Color;
        Debug.Log($"[NICKNAME] {photonView.Owner.NickName} Team {team}");
    }

    public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player targetPlayer, Hashtable changedProps)
    {
        if (targetPlayer != photonView.Owner)
            return;

        if (changedProps.ContainsKey("team"))
        {
            team = (int)changedProps["team"];
            ApplyColor();
        }
    }
}