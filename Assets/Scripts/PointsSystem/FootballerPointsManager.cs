using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CSV;
using UnityEngine;
using WebReader;

namespace DefaultNamespace
{
    public class FootballerPointsManager : MonoBehaviour
    {
        public static FootballerPointsManager Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        
        private enum FootballMatchEvent
        {
            Goal,
            Assist
        }

        public void GetFootballerPoints()
        {
            var footballPlayerPointsDatabaseList =
                CsvReader.LoadCsvFile(DashBoardManager.Instance.footballPlayerPointsDatabasePath);

            var clubNames = new List<string>();
            var footballPlayerPointsMap = new List<string[]>();
            for (int i = 0; i < footballPlayerPointsDatabaseList.Count; i++)
            {
                var split = footballPlayerPointsDatabaseList[i].Split(',');
                footballPlayerPointsMap.Add(split);
                
                if (!clubNames.Contains(split[0]))
                    clubNames.Add(split[0]);
            }

            var fixtureInfo = FixturePanelModule.Instance.GetFixtures();
            var fixtureMap = fixtureInfo.FixturesMap;
            
            //Test case;
            fixtureMap.Add("ft 1-0 Bayern Munich vs PSG", new FixtureEvents
            {
                GoalScorers = new Dictionary<string, int>
                {
                    {"Manuel Neuer", 1}
                }
            });

            foreach (var pair in fixtureMap)
            {
                var matchFixture = pair.Key;
                var matchFixtureEvents = pair.Value;

                var isChampionsTeam = IsChampionsTeam(clubNames, matchFixture);
                if (!isChampionsTeam)
                    continue;
                
                // Debug.LogError(matchFixture);
                
                var matchGoalScorers = matchFixtureEvents.GoalScorers;
                var matchAssists = matchFixtureEvents.Assists;

                if (matchGoalScorers != null)
                    UpdateFootballerPointsMap(matchGoalScorers, footballPlayerPointsMap, FootballMatchEvent.Goal);
    
                if (matchAssists != null)
                    UpdateFootballerPointsMap(matchAssists, footballPlayerPointsMap, FootballMatchEvent.Assist);
            }

            WriteToFootballerPointsDatabaseCsv(footballPlayerPointsMap);
        }

        private void UpdateFootballerPointsMap(Dictionary<string, int> playerEvents, List<string[]> footballPlayerPointsMap, FootballMatchEvent eventType)
        {
            foreach (var scorer in playerEvents)
            {
                var playerName = scorer.Key;
                var eventValue = scorer.Value;
                    
                // Debug.LogError("EventType: " + eventType + ", PlayerName: " + playerName + ", " + eventValue);

                var playerDetails = footballPlayerPointsMap.Select(x => x).Where(x => x[1] == playerName).ToList();
                if (playerDetails.Count == 0)
                    continue;

                var playerPoints = 0;
                switch (eventType)
                {
                    case FootballMatchEvent.Assist:
                        playerPoints = CalculateAssistPoints(eventValue);
                        break;
                    case FootballMatchEvent.Goal:
                        playerPoints = CalculateGoalPoints(eventValue);
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

        private void WriteToFootballerPointsDatabaseCsv(List<string[]> footballPlayerPointsMap)
        {
            using (StreamWriter writer = new StreamWriter("FootballPlayerPointsDatabase.csv"))
            {
                for (int i = 0; i < footballPlayerPointsMap.Count; i++)
                {
                    var line = string.Join(",", footballPlayerPointsMap[i]);
                    Debug.LogError(line);
                    
                    writer.WriteLine(line);
                }
                writer.Close();
            }
            // TEST^^^
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
    }
}