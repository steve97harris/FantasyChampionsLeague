using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Dashboard;
using DefaultNamespace;
using HtmlAgilityPack;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace WebReader
{
    public class WebClientReader : MonoBehaviour
    {
        public static WebClientReader Instance;
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        
        public const string FootballCriticLiveScoresUrl =
            "https://www.footballcritic.com/live-scores";
        
        #region Html Node Paths (footballcritic.com/live-scores)
        
        public const string MatchInfoList = "//ul[@class='info-list']";
        public const string MatchInfoStats = MatchInfoList + "/li[@class='matchInfoStats']";
        public const string InfoList = MatchInfoList + "/li[@class]";
        public const string HeadingBox = "//div[@class='heading-box']";
        public const string InfoBlockC323 = "//div[@class='info-block c_323']";
        public const string CompSpan = "//span[@id]";
        public const string TeamNames = "//span[@class='text']";
        
        #endregion

        public HtmlNodeCollection GetHtmlNodeCollection(string url, string xpath)
        {
            var web = new HtmlWeb();
            var doc = web.Load(url);
            var nodes = doc.DocumentNode.SelectNodes(xpath);

            return nodes;
        }

        public static string FootballCriticLiveScoresUrlByDate(string date)
        {
            return FootballCriticLiveScoresUrl + "?date=" + date;
        }
    }
}