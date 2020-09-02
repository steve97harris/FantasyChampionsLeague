using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace WebReader
{
    public class WebClientReader : MonoBehaviour
    {
        private const string Url =
            "https://www.bbc.co.uk/sport/football/scores-fixtures";

        private void Start()
        {
            
        }

        private IEnumerator GetWebText()
        {
            var www = UnityWebRequest.Get(Url);
            yield return www.SendWebRequest();
            
            if (www.isNetworkError || www.isHttpError)
                Debug.LogError(www.error);
            else
            {
                Debug.LogError(www.downloadHandler.text);

                var res = www.downloadHandler.text;
            }
        }
    }
}