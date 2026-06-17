using UnityEngine;
using UnityEngine.UI;
using TMPro; // Descomentá esta línea si usás TextMeshPro
using Photon.Pun;

public class DisplayText : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField; // Cambiá a InputField si usás el text clásico
    public TMP_Text persistentText;   // Cambiá a Text si usás el text clásico

    private const string PlayerPrefsNameKey = "SavedPlayerName";

    void Start()
    {
        // 1. Intentamos cargar el nombre guardado localmente en el dispositivo
        if (PlayerPrefs.HasKey(PlayerPrefsNameKey))
        {
            string savedName = PlayerPrefs.GetString(PlayerPrefsNameKey);

            // Lo mostramos en la UI
            inputField.text = savedName;
            persistentText.text = "Bienvenido, " + savedName;

            // Lo registramos en Photon inmediatamente
            SetPhotonNickName(savedName);
        }
        else
        {
            persistentText.text = "Ingresa tu nombre...";
        }
    }

    // Esta función se va a ejecutar cuando el usuario termine de escribir o pulse un botón de "Guardar/Conectar"
    public void Create()
    {
        string enteredName = inputField.text;

        // Validamos que no sea un nombre vacío
        if (string.IsNullOrEmpty(enteredName) || enteredName.Trim().Length == 0)
        {
            Debug.LogWarning("El nombre no puede estar vacío.");
            return;
        }

        // 2. Guardamos localmente para la próxima sesión
        PlayerPrefs.SetString(PlayerPrefsNameKey, enteredName);
        PlayerPrefs.Save();

        // 3. Actualizamos la UI local
        persistentText.text = "Nombre guardado: " + enteredName;

        // 4. Sincronizamos con la red de Photon
        SetPhotonNickName(enteredName);
    }

    private void SetPhotonNickName(string name)
    {
        // Asignamos el NickName en Photon. Esto se sincroniza automáticamente en la red.
        PhotonNetwork.NickName = name;
        Debug.Log($"Nombre de red sincronizado: {PhotonNetwork.NickName}");
    }
}