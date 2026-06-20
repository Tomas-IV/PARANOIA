using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine;
using System.Collections;
public class ChuckNorris : MonoBehaviour
{
    private string url = "https://api.chucknorris.io/jokes/random";

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            StartCoroutine(GetJoke((joke) =>
            {
                Debug.Log(joke);
            }));
        }
    }

    public IEnumerator GetJoke(System.Action<string> onResult)
    {
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error API: " + req.error);
                onResult?.Invoke("Error al obtener chiste");
            }
            else
            {
                string json = req.downloadHandler.text;

                ChuckNorrisData data =
                    JsonConvert.DeserializeObject<ChuckNorrisData>(json);

                onResult?.Invoke(data.value);
            }
        }

    }
}

[System.Serializable]
public class ChuckNorrisData
{
    public string value;
}