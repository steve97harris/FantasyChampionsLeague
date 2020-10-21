using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ErrorManagement;
using UnityEngine;
using UnityEngine.Serialization;
using WebReader;
using Firebase;
using Firebase.Storage;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class EventPointsRetriever : MonoBehaviour
    {
        public static EventPointsRetriever Instance;

        public void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                if (Instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
            DontDestroyOnLoad(this.gameObject.transform.parent.gameObject);
            
            var scene = SceneManager.GetActiveScene();
            if (scene.name != "ScoringSystem") 
                return;
            
            StartCoroutine(RunEventPointsUpdate());
        }

        private enum FootballMatchEvent
        {
            Goal,
            Assist
        }

        private IEnumerator RunEventPointsUpdate()
        {
            yield return new WaitForSeconds(1f);
            
            RunUpdate();
        }

        private async void RunUpdate()
        {
            var newDate = RetrieveYesterdaysDate();
            
            var fileMeta = await FirebaseDataStorage.Instance.DownloadMetaData(DashBoardManager.FileNameB);

            if (fileMeta.GetCustomMetadata("date") == null)
            {
                UpdateFirebaseFootballPlayerPointsDatabase(WebClientReader.FootballCriticLiveScoresUrl + "?date=" + newDate, newDate);
            }
            else
            {
                var lastUpdate = fileMeta.GetCustomMetadata("date");
                
                if (lastUpdate != newDate)
                    UpdateFirebaseFootballPlayerPointsDatabase(WebClientReader.FootballCriticLiveScoresUrl + "?date=" + newDate, newDate);
                else
                {
                    Debug.LogError("PlayerPointsDatabase is up to date, no changes to be made");
                }
            }
        }

        private async void UpdateFirebaseFootballPlayerPointsDatabase(string url, string date)
        {
            var stream = await FirebaseDataStorage.Instance.DownloadFileStreamAsync(DashBoardManager.FileNameB);
            var footballPlayerPointsDatabaseList = CsvReader.LoadCsvFileViaStream(stream);
            
            if (footballPlayerPointsDatabaseList.Count > 0)
                DebugLogFootballPlayerPointsDatabase(footballPlayerPointsDatabaseList);
            else
            {
                Debug.LogError("FootballPlayerPointsDatabaseList returned count = 0");
            }

            var clubNames = new List<string>();
            var footballPlayerPointsList = new List<string[]>();
            for (int i = 0; i < footballPlayerPointsDatabaseList.Count; i++)
            {
                var split = footballPlayerPointsDatabaseList[i].Split(',');
                footballPlayerPointsList.Add(split);
                
                if (!clubNames.Contains(split[0]))
                    clubNames.Add(split[0]);
            }

            var fixtureInfo = EventsAndFixturesModule.Instance.GetFixtures(url, WebClientReader.CompSpan);        // Get today's football fixtures and events
            var fixtureMap = fixtureInfo.FixturesMap;
            
            // Test Case:
            // fixtureMap.Add("PSG vs flkgnldknfgsd", new FixtureEvents
            // {
            //     GoalScorers = new Dictionary<string, int>
            //     {
            //         {"Neymar Jr", 23}
            //     }
            // });
            // --------------

            foreach (var pair in fixtureMap)
            {
                var matchFixture = pair.Key;
                var matchFixtureEvents = pair.Value;
                var matchLeague = pair.Value.League;
                
                if (!IsChampionsTeam(clubNames, matchFixture) && !IsChampionsLeagueFixture(matchLeague))
                {
                    Debug.Log("[Non FCL]: " + matchFixture + "{" + matchLeague + "}");
                    continue;
                }
                
                Debug.Log("[FCL]: " + matchFixture + "{" + matchLeague + "}");

                var matchGoalScorers = matchFixtureEvents.GoalScorers;
                var matchAssists = matchFixtureEvents.Assists;

                if (matchGoalScorers != null)
                    UpdateFootballPlayerPointsList(matchGoalScorers, footballPlayerPointsList, FootballMatchEvent.Goal);
    
                if (matchAssists != null)
                    UpdateFootballPlayerPointsList(matchAssists, footballPlayerPointsList, FootballMatchEvent.Assist);
            }
            
            var strList = new List<string>();
            for (int i = 0; i < footballPlayerPointsList.Count; i++)
            {
                var line = string.Join(",", footballPlayerPointsList[i]);
                strList.Add(line);
            }
            DebugLogFootballPlayerPointsDatabase(strList);

            var byteData = FirebaseDataStorage.Instance.ConvertStringArrayListToByteArray(strList);
            FirebaseDataStorage.Instance.UploadFromBytes(byteData, DashBoardManager.FileNameB);
            FirebaseDataStorage.Instance.UploadMetaData(DashBoardManager.FileNameB, date);
        }

        private void UpdateFootballPlayerPointsList(Dictionary<string, int> playerEvents, List<string[]> footballPlayerPointsMap, FootballMatchEvent eventType)
        {
            foreach (var scorer in playerEvents)
            {
                var playerName = scorer.Key;
                var eventValue = scorer.Value;
                    
                Debug.Log("EventType: " + eventType + ", PlayerName: " + playerName + ", " + eventValue);

                var playerDetails = footballPlayerPointsMap.Select(x => x).Where(x => x[1] == playerName).ToList();
                if (playerDetails.Count == 0)
                {
                    Debug.LogError(playerName + " not found");
                    continue;
                }

                var playerPoints = 0;
                switch (eventType)
                {
                    case FootballMatchEvent.Assist:
                        playerPoints = CalculateAssistPoints(eventValue);
                        Debug.LogError("assist points = " + playerPoints);
                        break;
                    case FootballMatchEvent.Goal:
                        playerPoints = CalculateGoalPoints(eventValue);
                        Debug.LogError("goal points = " + playerPoints);
                        break;
                }

                if (playerDetails.Count == 1)
                {
                    var arr = playerDetails[0];

                    int.TryParse(arr[2], out var totalPlayerPointsInt);
                    totalPlayerPointsInt += playerPoints;

                    arr[2] = totalPlayerPointsInt.ToString();
                }
                    
                if (playerDetails.Count > 1)
                {
                    Debug.LogError("Error: Two players with same name, Points for player not updated!!");
                }
            }
        }
        
        private string RetrieveYesterdaysDate()
        {
            var date = $"{DateTime.Now.Date.AddDays(-1):yyyy-MM-dd}";
            return date;
        }

        private bool IsChampionsTeam(List<string> clubNames, string matchFixture)
        {
            var championsTeamPlayingInFixture = false;
            for (int i = 0; i < clubNames.Count; i++)
            {
                var clubName = Regex.Replace(clubNames[i], "([a-z])([A-Z])", "$1 $2");
                if (matchFixture.Contains(clubName))
                    championsTeamPlayingInFixture = true;
            }

            return championsTeamPlayingInFixture;
        }

        private bool IsChampionsLeagueFixture(string league)
        {
            return league.Contains("UEFA Champions League");
        }

        private int CalculateGoalPoints(int numberOfGoals)
        {
            const int pointsPerGoal = 6;
            return numberOfGoals * pointsPerGoal;
        }

        private int CalculateAssistPoints(int numberOfAssists)
        {
            const int pointsPerAssist = 4;
            return numberOfAssists * pointsPerAssist;
        }

        private void DebugLogFootballPlayerPointsDatabase(List<string> footballPlayerPointsDatabaseList)
        {
            var str = "";
            for (int i = 0; i < footballPlayerPointsDatabaseList.Count; i++)
            {
                str += footballPlayerPointsDatabaseList[i] + Environment.NewLine;
            }
            Debug.Log(str);
        }

        private List<string[]> ResetAllFootballPlayersPoints(List<string[]> footballPlayerPointsList, string newPointValue)
        {
            for (int i = 0; i < footballPlayerPointsList.Count; i++)
            {
                footballPlayerPointsList[i][2] = newPointValue;
            }
            return footballPlayerPointsList;
        }
    }
}