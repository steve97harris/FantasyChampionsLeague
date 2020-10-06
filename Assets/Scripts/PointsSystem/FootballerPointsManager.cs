using System;
using System.Collections.Generic;
using System.Linq;
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

        public void GetFootballerPoints()
        {
            var footballPlayerPointsDatabaseList =
                CsvReader.LoadCsvFile(Application.streamingAssetsPath + "/FootballPlayerPointsDatabase.csv");

            var footballPlayerPointsMap = new List<string[]>();
            for (int i = 0; i < footballPlayerPointsDatabaseList.Count; i++)
            {
                var split = footballPlayerPointsDatabaseList[i].Split(',');
                footballPlayerPointsMap.Add(split);
            }

            var fixtureInfo = FixturePanelModule.Instance.GetFixtures();
            var fixtureMap = fixtureInfo.FixturesMap;

            foreach (var pair in fixtureMap)
            {
                var matchFixture = pair.Key;
                var matchFixtureEvents = pair.Value;

                var matchGoalScorers = matchFixtureEvents.GoalScorers;
                var matchAssists = matchFixtureEvents.Assists;
                foreach (var scorer in matchGoalScorers)
                {
                    var playerName = scorer.Key;
                    var goalsScored = scorer.Value;

                    var playerDetails = footballPlayerPointsMap.Select(x => x).Where(x => x[1] == playerName).ToList();
                    if (playerDetails.Count == 0)
                        continue;

                    var playerGoalPoints = CalculateGoalPoints(goalsScored);

                    if (playerDetails.Count == 1)
                    {
                        var arr = playerDetails[0];

                        int.TryParse(arr[2], out var totalPlayerPointsInt);
                        totalPlayerPointsInt += playerGoalPoints;

                        arr[2] = totalPlayerPointsInt.ToString();
                    }
                    
                    if (playerDetails.Count > 1)
                    {
                        Debug.LogError("Error: Two players with same name, Points for player not updated!!");
                    }
                }

                foreach (var assistMaker in matchAssists)
                {
                    var playerName = assistMaker.Key;
                    var assistsMade = assistMaker.Value;

                    var playerDetails = footballPlayerPointsMap.Select(x => x).Where(x => x[1] == playerName).ToList();
                    if (playerDetails.Count == 0)
                        continue;

                    var assistPoints = CalculateAssistPoints(assistsMade);

                    if (playerDetails.Count == 1)
                    {
                        var arr = playerDetails[0];

                        int.TryParse(arr[2], out var totalPlayerPointsInt);
                        totalPlayerPointsInt += assistPoints;

                        arr[2] = totalPlayerPointsInt.ToString();
                    }

                    if (playerDetails.Count > 1)
                    {
                        Debug.LogError("Error: Two players with same name, Points for player not updated!!");
                    }
                }
                
                // NEEDS TESTING^^^
            }
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