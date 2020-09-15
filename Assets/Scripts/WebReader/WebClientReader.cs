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
            var nodes = doc.DocumentNode.SelectNodes(TeamInfo);
            
            var fixtures = new List<string>();
            var goalsList = new List<string>();

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                var currNode = nodes[i].InnerText;
                currNode = RemoveWhitespace(currNode);

                if (currNode.Contains("&nbsp;"))
                {
                    goalsList.Add(currNode);
                }
                else
                {
                    currNode = currNode.Replace(",", " ");
                    fixtures.Add(currNode);
                }
            }

            var goalScorers = new Dictionary<string, int>();
            var assists = new Dictionary<string, int>();
            for (int i = 0; i < goalsList.Count; i++)
            {
                ExtractNames(goalsList[i], goalScorers, assists);
            }
            
            
        }

        private void ExtractNames(string goalInfo, IDictionary<string, int> goalScorers, IDictionary<string, int> assists) 
        {
            goalInfo = goalInfo.Replace(",", "");
            var regex = new Regex(@"([a-zA-Z\w]+)");
            var match = regex.Match(goalInfo);

            GetAllMatches(match, goalScorers, assists);
        }
        
        private void GetAllMatches(Match match, IDictionary<string, int> goalScorers, IDictionary<string, int> assists)
        {
            var str = match.Groups[1].Value;
            
            AddToGoalAssistMaps(str, goalScorers, assists);

            var nextMatch = match.NextMatch();
            if (nextMatch.Success)
            {
                GetAllMatches(nextMatch, goalScorers, assists);
            }
        }

        private void AddToGoalAssistMaps(string playerName, IDictionary<string, int> goalScorers, IDictionary<string, int> assists)
        {
            var containDigit = playerName.Any(char.IsDigit);
            if (containDigit)
                return;
            
            if (playerName.Contains("nbsp"))
                return;
            
            if (playerName.StartsWith("assistby"))
            {
                playerName = RemoveAssistText(playerName);
                
                if (!assists.ContainsKey(playerName))
                    assists.Add(playerName, 1);
                else
                    assists[playerName]++;
            }
            else
            {
                if (!goalScorers.ContainsKey(playerName))
                    goalScorers.Add(playerName, 1);
                else
                    goalScorers[playerName]++;
            }
        }
        
        private string RemoveWhitespace(string str)
        {
            var res = Regex.Replace(str, @"\s+", ",");
                
            if (res[0] == ',')
                res = res.Remove(0, 1);

            return res;
        }

        private string RemoveAssistText(string s)
        {
            var startIndexOfAssistBy = s.IndexOf("assistby", StringComparison.Ordinal);
            s = s.Remove(startIndexOfAssistBy, "assistby".Length);
            return s;
        }
    }
}