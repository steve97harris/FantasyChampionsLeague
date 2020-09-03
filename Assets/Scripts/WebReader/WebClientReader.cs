using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using DefaultNamespace;
using HtmlAgilityPack;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace WebReader
{
    public class WebClientReader : MonoBehaviour
    {
        private const string Url =
            "https://www.footballcritic.com/live-scores";
        
        #region Html Node Paths (footballcritic.com/live-scores)
        
        private const string MatchInfoList = "//ul[@class='info-list']";
        private const string MatchInfoStats = MatchInfoList + "/li[@class='matchInfoStats']";
        private const string TeamInfo = MatchInfoList + "/li[@class]";
        
        #endregion

        private void Start()
        {
            GetWebText();
        }

        private void GetWebText()
        {
            var web = new HtmlWeb();
            var doc = web.Load(Url);
            //Debug.LogError(doc.Text);
            var nodes = doc.DocumentNode.SelectNodes(TeamInfo);
            
            // var splitMap = new Dictionary<>();
            var nodeList = new List<string>();
            foreach (var node in nodes)
            {
                var nodeString = node.InnerText;
                // var split = Regex.Split(nodeString,Environment.NewLine).Select(x => x.Trim()).ToArray();
                var split = Regex.Replace(nodeString, @"\s+", ",");
                
                nodeList.Add(split);
            }

            for (int i = 0; i < nodeList.Count; i++)
            {
                Debug.LogError(nodeList[i]);
                // for (int j = 0; j < nodeList[i].Length; j++)
                // {
                //     Debug.LogError(nodeList[i][j]);
                // }
            }
        }
    }
}