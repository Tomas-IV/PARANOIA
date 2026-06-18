using UnityEngine.Networking;
using Newtonsoft.Json;
using UnityEngine;
using System.Collections;
public class ChuckNorrisJokeConsumer : MonoBehaviour
{
    private string url = "https://api.chucknorris.io/jokes/random";
    public IEnumerator GetJoke()
    {
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            yield return req.SendWebRequest();
        }
    }
}
public class ChuckNorrisData
{
}