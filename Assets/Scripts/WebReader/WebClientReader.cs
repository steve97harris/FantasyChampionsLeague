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
            
            var fullTimeScores = new List<string>();
            var goalsList = new List<string>();

            for (int i = 0; i < nodes.Count - 1; i++)
            {
                var currNode = nodes[i].InnerText;
                currNode = RemoveWhitespace(currNode);

                if (currNode.StartsWith("ft"))
                {
                    fullTimeScores.Add(currNode);
                }
                
                if (currNode.Contains("&nbsp;,"))
                {
                    goalsList.Add(currNode);
                }
            }

            var goalScorers = new Dictionary<string, int>();
            var assists = new Dictionary<string, int>();
            for (int i = 0; i < goalsList.Count; i++)
            {
                ExtractNames(goalsList[i], goalScorers, assists);
            }
            
            foreach (var pair in goalScorers)
            {
                Debug.LogError(pair.Key + ", " + pair.Value);
            }
            
            Debug.LogError("----------");
            foreach (var pair in assists)
            {
                Debug.LogError(pair.Key + ", " + pair.Value);
            }
        }

        private string RemoveWhitespace(string str)
        {
            var res = Regex.Replace(str, @"\s+", ",");
                
            if (res[0] == ',')
                res = res.Remove(0, 1);

            return res;
        }
        
        private void ExtractNames(string arg, IDictionary<string, int> goalScorers, IDictionary<string, int> assists) 
        {
            var regex = new Regex(@"([a-zA-Z\w]+)");
            var match = regex.Match(arg);

            arg = arg.Replace(",", "");
            var currName = "";
            GetGameInfo(currName, match, goalScorers, assists, arg);
        }

        // Needs changing to account for players with 3 names <- Needs testing
        // Needs changing to account for disallowed goals
        private void GetGameInfo(string currName, Match match, IDictionary<string, int> goalScorers, IDictionary<string, int> assists, string arg)
        {
            var temp = currName + match.Groups[1].Value;
                
            if (!temp.StartsWith("by"))
            {
                if (arg.Contains(temp))
                {
                    currName += match.Groups[1].Value;
                    AddToGoalAssistMaps(currName, arg, goalScorers, assists);
                }
                    
                if (!arg.Contains(temp))
                {
                    AddToGoalAssistMaps(currName, arg, goalScorers, assists);
                    currName = "";
                }
            }
            
            var nextMatch = match.NextMatch();
            
            if (!nextMatch.Success) 
                return;

            GetGameInfo(currName, nextMatch, goalScorers, assists, arg);
        }

        private void AddToGoalAssistMaps(string fullName, string arg, IDictionary<string, int> goalScorers, IDictionary<string, int> assists)
        {
            var assistFullName = "by" + fullName;
            
            var containDigit = fullName.Any(char.IsDigit);
            if (containDigit)
                return;
            
            if (arg.Contains(assistFullName))
            {
                if (!assists.ContainsKey(fullName))
                    assists.Add(fullName, 1);
                else
                    assists[fullName]++;
            }
            else
            {
                if (!goalScorers.ContainsKey(fullName))
                    goalScorers.Add(fullName, 1);
                else
                    goalScorers[fullName]++;
            }
        }
    }
}