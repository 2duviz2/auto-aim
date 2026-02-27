namespace Mod.Helpers;

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class WebRequests
{
    static int timeout = 15;

    public static IEnumerator GetStringFromUrl(string url, System.Action<string> callback)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            www.timeout = timeout;
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Plugin.LogError("Failed to load string: " + www.error);
                callback?.Invoke(null);
            }
            else
            {
                callback?.Invoke(www.downloadHandler.text);
            }
        }
    }

    public static IEnumerator PostRequest(string url, Dictionary<string, string> postData, System.Action<string> callback)
    {
        WWWForm form = new WWWForm();
        foreach (var pair in postData)
        {
            form.AddField(pair.Key, pair.Value);
        }
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            www.timeout = timeout;
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Plugin.LogError("Failed to post request: " + www.error);
                callback?.Invoke(null);
            }
            else
            {
                callback?.Invoke(www.downloadHandler.text);
            }
        }
    }
}