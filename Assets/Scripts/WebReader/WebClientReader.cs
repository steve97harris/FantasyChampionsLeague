using System;
using System.Net;
using UnityEngine;

namespace WebReader
{
    public class WebClientReader : MonoBehaviour
    {
        public void Start()
        {
            var bbcSportsFootballData = WebDataString("https://www.bbc.co.uk/sport/football/scores-fixtures");
            
            Debug.LogError(bbcSportsFootballData);
        }

        private string WebDataString(string address)
        {
            WebClient web = new WebClient();
            System.IO.Stream stream = web.OpenRead(address);

            var text = "";
            using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
            {
                text = reader.ReadToEnd();
            }

            return text;
        }
    }
}