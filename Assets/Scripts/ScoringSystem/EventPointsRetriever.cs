using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        public static string EventDate;

        private enum FootballMatchEvent
        {
            Goal,
            Assist
        }
        
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
                    Destroy(gameObject);
                }
            }
            DontDestroyOnLoad(gameObject.transform.parent.gameObject);
            
            var scene = SceneManager.GetActiveScene();
            if (scene.name != "ScoringSystem") 
                return;
            
            EventDate = RetrieveYesterdaysDate();
            StartCoroutine(RunEventPointsUpdate(EventDate));
        }
        
        private IEnumerator RunEventPointsUpdate(string date)
        {
            yield return new WaitForSeconds(1f);

            var url = WebClientReader.FootballCriticLiveScoresUrlByDate(date);
            
            RunUpdate(date, url);
        }

        private async void RunUpdate(string date, string url)
        {
            var fileMeta = await FirebaseDataStorage.Instance.DownloadMetaData(DashBoardManager.FileNameB);

            if (fileMeta.GetCustomMetadata("date") == null)
            {
                UploadNewPointsData(url, date);
            }
            else
            {
                var lastUpdate = fileMeta.GetCustomMetadata("date");
                
                if (lastUpdate != date)
                    UploadNewPointsData(url, date);
                else
                {
                    Debug.LogError("PlayerPointsDatabase is up to date, no changes to be made");
                }
            }
        }

        private async void DisplayNewPointsData(string url)
        {
            var updatedPointsData = await RetrieveUpdatedPointsData(url);
            DebugLogPointsData(updatedPointsData);
        }

        private async void UploadNewPointsData(string url, string date)
        {
            var updatedPointsDataList = await RetrieveUpdatedPointsData(url);
            
            var byteData = FirebaseDataStorage.Instance.ConvertStringArrayListToByteArray(updatedPointsDataList);
            
            FirebaseDataStorage.Instance.UploadFromBytes(byteData, DashBoardManager.FileNameB);
            FirebaseDataStorage.Instance.UploadMetaData(DashBoardManager.FileNameB, date);
        }

        private async Task<List<string>> RetrieveUpdatedPointsData(string url)
        {
            var stream = await FirebaseDataStorage.Instance.DownloadFileStreamAsync(DashBoardManager.FileNameB);
            var footballPlayerPointsDatabaseList = CsvReader.LoadCsvFileViaStream(stream);
            var fixtureInfo = EventsAndFixturesModule.Instance.GetFixtures(url, WebClientReader.CompSpan); 
            return GetUpdatedPointsDataList(footballPlayerPointsDatabaseList, fixtureInfo);;
        }

        private List<string> GetUpdatedPointsDataList(List<string> footballPlayerPointsDatabaseList, Fixtures fixtureInfo)
        {
            if (footballPlayerPointsDatabaseList.Count > 0)
                DebugLogPointsData(footballPlayerPointsDatabaseList);
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
                    UpdatePointsDataList(matchGoalScorers, footballPlayerPointsList, FootballMatchEvent.Goal);
    
                if (matchAssists != null)
                    UpdatePointsDataList(matchAssists, footballPlayerPointsList, FootballMatchEvent.Assist);
            }
            
            var updatedPointsList = new List<string>();
            for (int i = 0; i < footballPlayerPointsList.Count; i++)
            {
                var line = string.Join(",", footballPlayerPointsList[i]);
                updatedPointsList.Add(line);
            }

            return updatedPointsList;
        }

        private void UpdatePointsDataList(Dictionary<string, int> playerEvents, List<string[]> footballPlayerPointsMap, FootballMatchEvent eventType)
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

        private void DebugLogPointsData(List<string> footballPlayerPointsDatabaseList)
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