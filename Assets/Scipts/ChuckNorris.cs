using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;

public class ChuckNorris : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject jokeCanvas;
    [SerializeField] private TextMeshProUGUI instrusionText;
    [SerializeField] private TextMeshProUGUI jokeText;
    [SerializeField] private Image jokeImage;

    [Header("Settings")]
    [SerializeField] private float visibleTime = 5f;

    private const string URL = "https://api.chucknorris.io/jokes/random";

    private Coroutine hideCoroutine;

    private void Start()
    {
        jokeCanvas.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            instrusionText.gameObject.SetActive(false);
            StartCoroutine(GetJoke());
        }
    }

    private IEnumerator GetJoke()
    {
        using (UnityWebRequest req = UnityWebRequest.Get(URL))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + req.error);
                yield break;
            }

            string json = req.downloadHandler.text;

            ChuckNorrisData data =
                JsonConvert.DeserializeObject<ChuckNorrisData>(json);

            jokeText.text = data.value;

            jokeCanvas.SetActive(true);

            yield return StartCoroutine(LoadImage(data.icon_url));

            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }

            hideCoroutine = StartCoroutine(HideCanvas());
        }
    }

    private IEnumerator LoadImage(string imageUrl)
    {
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al cargar imagen: " + req.error);
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(req);

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            jokeImage.sprite = sprite;
        }
    }

    private IEnumerator HideCanvas()
    {
        yield return new WaitForSeconds(visibleTime);

        jokeCanvas.SetActive(false);
        instrusionText.gameObject.SetActive(true);
    }
}

[System.Serializable]
public class ChuckNorrisData
{
    public string value;
    public string icon_url;
}