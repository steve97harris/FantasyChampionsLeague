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

            var footballPlayerPointsMap = new Dictionary<string, int>();
            for (int i = 0; i < footballPlayerPointsDatabaseList.Count; i++)
            {
                var split = footballPlayerPointsDatabaseList[i].Split(',');
                var club = split[0];
                var playerName = split[1];
                var totalPoints = split[2];
                int.TryParse(totalPoints, out var totalPointInt);

                if (!footballPlayerPointsMap.ContainsKey(playerName))
                {
                    footballPlayerPointsMap.Add(playerName, totalPointInt);
                }
                else
                    Debug.LogError("FootballPlayerPointsMap already contains: " + playerName);
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

                    if (!footballPlayerPointsMap.ContainsKey(playerName)) 
                        continue;
                    
                    var goalPoints = CalculateGoalPoints(goalsScored);

                    footballPlayerPointsMap[playerName] += goalPoints;
                }

                foreach (var assistMaker in matchAssists)
                {
                    var playerName = assistMaker.Key;
                    var assistsMade = assistMaker.Value;

                    if (!footballPlayerPointsMap.ContainsKey(playerName)) 
                        continue;
                    
                    var assistPoints = CalculateAssistPoints(assistsMade);

                    footballPlayerPointsMap[playerName] += assistPoints;
                }
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