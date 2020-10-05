using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HtmlAgilityPack;
using TMPro;
using UnityEngine;
using WebReader;

namespace DefaultNamespace
{
    public class FixturePanelModule : MonoBehaviour
    {
        public static FixturePanelModule Instance;
        
        private string _currentFixture = "";
        private readonly Dictionary<string, int> _goalScorers = new Dictionary<string, int>();
        private readonly Dictionary<string, int> _assists = new Dictionary<string, int>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        private void Start()
        {
            SetFixturesPanel();
            FootballerPointsManager.Instance.GetFootballerPoints();
        }

        public void SetFixturesPanel()
        {
            var fixtureInfo = GetFixtures();
            var fixtures = fixtureInfo.FixturesList;
            
            SetFixturesUi(fixtures);
        }
        
        public Fixtures GetFixtures()
        {
            var footballCriticUrl = WebClientReader.FootballCriticLiveScoresUrl;
            var compSpanPath = WebClientReader.CompSpan;
            var compSpanNodes = WebClientReader.Instance.GetHtmlNodeCollection(footballCriticUrl, compSpanPath);

            var infoList = ReadNodes(compSpanNodes);
            
            var teams = GetTeamsPlayingToday();
            
            var fixture = "";
            var fixtures = new List<string>();
            var fixtureMap = new Dictionary<string, FixtureEvents>();

            for (int i = 0; i < infoList.Count; i++)
            {
                if (infoList[i].StartsWith("-"))
                    continue;

                if (infoList[i].Contains("&nbsp;"))
                {
                    ExtractNames(infoList[i]);

                    if (fixture == "")
                        Debug.LogError("Fixture returned null");

                    var goalScorers = new Dictionary<string, int>(_goalScorers);
                    var assists = new Dictionary<string, int>(_assists);
                    var fixtureEvents = new FixtureEvents { GoalScorers = goalScorers, Assists = assists };

                    if (!fixtureMap.ContainsKey(_currentFixture))
                        fixtureMap.Add(_currentFixture, fixtureEvents);

                    _goalScorers.Clear();
                    _assists.Clear();
                }
                else
                {
                    fixture = FixFixtureString(infoList[i], teams);

                    if (IsInvalidFixture(fixture)) 
                        continue;
                    
                    fixtures.Add(fixture);
                    
                    _currentFixture = fixture;
                }
            }
            
            var fixtureModule = new Fixtures{ FixturesList = fixtures, FixturesMap = fixtureMap };
            return fixtureModule;
        }

        private List<string> ReadNodes(HtmlNodeCollection nodes)
        {
            var infoList = new List<string>();
            for (int i = 0; i < nodes.Count; i++)
            {
                if (!nodes[i].Id.Contains("compspan"))
                    continue;
                
                var headerNode = nodes[i].SelectSingleNode("./div[@class='heading-box']");
                var matchInfoCollection = nodes[i].SelectNodes("./following::ul/li");
                
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

            return infoList;
        }
        
        private void SetFixturesUi(List<string> fixtures)
        {
            var fixtureTemplate = Resources.Load<GameObject>("Prefabs/FixturesPanel/FixtureTemplate");
            var fixtureContent = GameObjectFinder.FindSingleObjectByName("FixturesContent");
            
            foreach (Transform child in fixtureContent.transform)
            {
                Destroy(child.gameObject);
            }
            
            for (int i = 0; i < fixtures.Count; i++)
            {
                var fixtureTemp = Instantiate(fixtureTemplate, fixtureContent.transform);
                fixtureTemp.GetComponentInChildren<TMP_Text>().text = fixtures[i];
                
                if (fixtures[i].Contains('('))
                {
                    fixtureTemp.GetComponentInChildren<TMP_Text>().fontStyle = FontStyles.Underline;
                }
            }
        }

        private string FixFixtureString(string fixture, List<string> teams)
        {
            fixture = fixture.Replace(",", " ");
            fixture = DiacriticsRemover.RemoveDiacritics(fixture);
                
            for (int j = 0; j < teams.Count; j++)
            {
                if (fixture.EndsWith(teams[j]))
                {
                    fixture = fixture.Replace(teams[j], "vs " + teams[j]);
                }
            }

            fixture = UpdateMatchTime(fixture);

            return fixture;
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

        private bool IsInvalidFixture(string fixture)
        {
            var fixtureSplit = fixture.Split(' ');
            var isInt = int.TryParse(fixtureSplit[0], out var res);

            return fixtureSplit.Length == 3 && isInt;
        }

        private void ExtractNames(string goalInfo) 
        {
            goalInfo = goalInfo.Replace(",", "");
            var regex = new Regex(@"([a-zA-Z\w \t\r\n\-]+)");    // Regular Expression to match player names 
            var match = regex.Match(goalInfo);
            
            GetAllRegexMatchesInString(match);
        }

        private void GetAllRegexMatchesInString(Match match)
        {
            var str = match.Groups[1].Value;
            
            AddToGoalAssistMaps(str);

            var nextMatch = match.NextMatch();
            if (nextMatch.Success)
            {
                GetAllRegexMatchesInString(nextMatch);
            }
        }
        
        private void AddToGoalAssistMaps(string playerName)
        {
            playerName = UpdatePlayerNameString(playerName);

            if (playerName.Contains("nbsp") || playerName == "")
                return;
            
            if (playerName.StartsWith("assistby"))
            {
                playerName = RemoveAssistText(playerName);

                if (!_assists.ContainsKey(playerName))
                    _assists.Add(playerName, 1);
                else
                    _assists[playerName]++;
                
                // Debug.LogError("Assist: " + playerName);
            }
            else
            {
                if (!_goalScorers.ContainsKey(playerName))
                {
                    _goalScorers.Add(playerName, 1);
                }
                else
                {
                    _goalScorers[playerName]++;
                }
                
                // Debug.LogError("Goal Scored: " + playerName);
            }
        }

        private string UpdatePlayerNameString(string playerName)
        {
            var containsDigit = playerName.Any(char.IsDigit);
            if (containsDigit)
                playerName = Regex.Replace(playerName, @"[\d-]", string.Empty);

            playerName = DiacriticsRemover.RemoveDiacritics(playerName);
            playerName = Regex.Replace(playerName, "([a-z])([A-Z])", "$1 $2");

            return playerName;
        }

        private List<string> GetTeamsPlayingToday()
        {
            var footballCriticUrl = WebClientReader.FootballCriticLiveScoresUrl;
            var teamNameXPath = WebClientReader.TeamNames;
            var nodeCollection = WebClientReader.Instance.GetHtmlNodeCollection(footballCriticUrl, teamNameXPath);

            var teams = new List<string>();
            for (int i = 0; i < nodeCollection.Count; i++)
            {
                var teamName = nodeCollection[i].InnerText;
                teamName = RemoveWhitespace(teamName);
                teamName = teamName.Replace(",", " ");
                teamName = teamName.Replace(".", "");
                teamName = DiacriticsRemover.RemoveDiacritics(teamName);
                
                if (!IsTeamName(teamName))
                    continue;

                if (!teams.Contains(teamName))
                    teams.Add(teamName);
                else
                    Debug.LogError("Teams list already contains team: " + teamName);
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