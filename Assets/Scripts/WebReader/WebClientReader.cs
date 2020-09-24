﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
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
        private const string InfoList = MatchInfoList + "/li[@class]";
        private const string HeadingBox = "//div[@class='heading-box']";
        private const string InfoBlockC323 = "//div[@class='info-block c_323']";
        private const string CompSpan = "//span[@id]";
        
        #endregion

        private void Start()
        {
            GetWebText();
        }

        private void GetWebText()
        {
            var web = new HtmlWeb();
            var doc = web.Load(Url);
            var compSpanNodes = doc.DocumentNode.SelectNodes(CompSpan);

            var infoList = new List<string>();
            for (int i = 0; i < compSpanNodes.Count; i++)
            {
                if (!compSpanNodes[i].Id.Contains("compspan"))
                    continue;
                
                var headerNode = compSpanNodes[i].SelectSingleNode("./div[@class='heading-box']");
                var matchInfoCollection = compSpanNodes[i].SelectNodes("./following::ul/li");
                
                if (infoList.Count > matchInfoCollection.Count)
                {
                    var diff = infoList.Count - matchInfoCollection.Count;
                    infoList.RemoveRange(diff,infoList.Count - diff);
                }
                
                infoList.Add(RemoveWhitespace(headerNode.InnerText));
                for (int j = 0; j < matchInfoCollection.Count; j++)
                {
                    var matchInfo = RemoveWhitespace(matchInfoCollection[j].InnerText);
                    infoList.Add(matchInfo);
                }
            }

            var gamesWithoutTime = 0;
            var fixtures = new List<string>();
            var goalAssistList = new List<string>();
            for (int i = 0; i < infoList.Count; i++)
            {
                if (infoList[i].StartsWith("-"))
                    gamesWithoutTime++;
                
                if (infoList[i].Contains("&nbsp;"))
                    goalAssistList.Add(infoList[i]);
                else
                    fixtures.Add(infoList[i]);
            }
            fixtures.RemoveRange(fixtures.Count - gamesWithoutTime, gamesWithoutTime);

            var goalScorers = new Dictionary<string, int>();
            var assists = new Dictionary<string, int>();
            for (int i = 0; i < goalAssistList.Count; i++)
            {
                ExtractNames(goalAssistList[i], goalScorers, assists);
            }
            
            SetFixturesUi(fixtures);
        }
        
        private void SetFixturesUi(List<string> fixtures)
        {
            var fixtureTemplate = Resources.Load<GameObject>("Prefabs/FixturesPanel/FixtureTemplate");
            var fixtureContent = GameObjectFinder.FindSingleObjectByName("FixturesContent");
            
            foreach (Transform child in fixtureContent.transform)
            {
                Destroy(child.gameObject);
            }

            var teams = GetTeamsPlayingToday();

            for (int i = 0; i < fixtures.Count; i++)
            {
                fixtures[i] = fixtures[i].Replace(",", " ");
                fixtures[i] = DiacriticsRemover.RemoveDiacritics(fixtures[i]);
                
                for (int j = 0; j < teams.Count; j++)
                {
                    if (fixtures[i].EndsWith(teams[j]))
                    {
                        fixtures[i] = fixtures[i].Replace(teams[j], "vs " + teams[j]);
                    }
                }

                fixtures[i] = UpdateMatchTime(fixtures[i]);

                var fixtureTemp = Instantiate(fixtureTemplate, fixtureContent.transform);
                fixtureTemp.GetComponentInChildren<TMP_Text>().text = fixtures[i];
                
                if (fixtures[i].Contains('('))
                {
                    fixtureTemp.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Underline;
                }
            }
        }

        private void ExtractNames(string goalInfo, IDictionary<string, int> goalScorers, IDictionary<string, int> assists) 
        {
            goalInfo = goalInfo.Replace(",", "");
            var regex = new Regex(@"([a-zA-Z\w]+)");
            var match = regex.Match(goalInfo);

            GetAllMatches(match, goalScorers, assists);
        }

        private string UpdateMatchTime(string fixture)
        {
            var timeCheck = fixture.Substring(0, 5);
            
            DateTime.TryParse(timeCheck, out var result);
            var time = result.ToString("HH:mm");
            
            if (time == "00:00")
                return fixture;

            int.TryParse(time.Substring(0, 2), out var hour);
            var newHour = "";
            if (hour == 24)
            {
                newHour = "00";
            }
            else
            {
                hour += 1;
                
                if (hour > 9)
                    newHour = hour.ToString();
                else
                    newHour = "0" + hour;
            }

            fixture = fixture.Remove(0, 2);
            fixture = fixture.Insert(0, newHour);

            return fixture;
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
        
        private List<string> GetTeamsPlayingToday()
        {
            var web = new HtmlWeb();
            var doc = web.Load(Url);
            var nodeCollection = doc.DocumentNode.SelectNodes("//span[@class='text']");
            
            var teams = new List<string>();
            for (int i = 0; i < nodeCollection.Count; i++)
            {
                var nodeString = nodeCollection[i].InnerText;
                nodeString = RemoveWhitespace(nodeString);
                nodeString = nodeString.Replace(",", " ");
                nodeString = nodeString.Replace(".", "");
                nodeString = DiacriticsRemover.RemoveDiacritics(nodeString);
                
                if (!IsTeamName(nodeString))
                    continue;

                if (!teams.Contains(nodeString))
                    teams.Add(nodeString);
                else
                    Debug.LogError("Teams list already contains team");
            }

            return teams;
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

        private bool IsTeamName(string str)
        {
            var isInt = int.TryParse(str, out var result);
            var startWithInt = int.TryParse(str[0].ToString(), out var res);
            return !isInt && !startWithInt && !string.IsNullOrWhiteSpace(str) && !str.Contains(':') && str != "-" && !str.Contains('(') && !str.Contains(')');
        }
    }
}