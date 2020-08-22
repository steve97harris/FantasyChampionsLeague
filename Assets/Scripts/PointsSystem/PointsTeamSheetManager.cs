using System;
using System.Collections.Generic;
using System.Linq;
using CSV;
using Dashboard;
using GoogleSheetsLevelSynchronizer;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    public class PointsTeamSheetManager : MonoBehaviour
    {
        private int _currentGameweekIndex = 0;
        private static int _noOfGameweeks = 0;
        
        /// <summary> PointsTeamSheetPlayerMap - map of player prefabs 
        /// <typeparam name="Key">Football Player Name</typeparam>
        /// <typeparam name="Value">Football Player Prefab</typeparam>
        /// </summary>
        private static readonly Dictionary<string, GameObject> PointsTeamSheetPlayerMap = new Dictionary<string, GameObject>();
        
        /// <summary> GameweekAllPlayerPointsMap - map of all player points over each gameweek
        /// <typeparam name="Key">Football Player Name</typeparam>
        /// <typeparam name="Value">Football Player Points per gameweek</typeparam>
        /// E.g:
        /// Value[0] = individual player points at gameweek 0.
        /// Value[1] = individual player points at gameweek 1.
        /// </summary>
        private static readonly Dictionary<string,string[]> GameweekAllPlayerPointsMap = new Dictionary<string, string[]>();

        /// <summary> _gameweekTeamSheetPointsMap - array of dictionaries containing teamsheet player points.
        /// E.g: 
        /// _gameweekTeamSheetPointsMap[0] = teamsheet player points for gameweek 0.
        /// _gameweekTeamSheetPointsMap[1] = teamsheet player points for gameweek 1.
        /// <typeparam name="Key">Football Player Name</typeparam>
        /// <typeparam name="Value">Football Player Points</typeparam>
        /// </summary>
        private static Dictionary<string, int>[] _gameweekTeamSheetPointsArray; 

        public void Start()
        { 
            GetPlayerMap(); 
            GetGameweekPoints();
            
            // _gameweekTeamSheetPointsArray = new Dictionary<string, int>[_noOfGameweeks];
            //
            // for (int i = 0; i < _noOfGameweeks; i++)
            // {
            //     SetGwTeamSheetPointsArray(i);
            // }
            // SetHeadCoachPoints();
            //
            // SetPlayerPointsUi(0);
        }
        
        private void GetPlayerMap()
        {
            var pointsTeamSheet = GameObjectFinder.FindSingleObjectByName("PointsTeamSheet");
            var grandChildrenOfTeamSheet = GetGreatGrandChildren(pointsTeamSheet);

            for (int i = 0; i < grandChildrenOfTeamSheet.Count; i++)
            {
                var playerObj = grandChildrenOfTeamSheet[i];

                if (playerObj.GetComponent<FootballPlayerDetails>() == null)
                    continue;
                var playerName = playerObj.GetComponent<FootballPlayerDetails>().playerName;
                
                if (!PointsTeamSheetPlayerMap.ContainsKey(playerName))
                    PointsTeamSheetPlayerMap.Add(playerName, playerObj);
            }
        }
        
        private void GetGameweekPoints()
        {
            
        }
        
        private void SetGwTeamSheetPointsArray(int currentGwIndex)
        {
            var currentGwTeamSheetPointsMap = new Dictionary<string,int>();
            foreach (var pair1 in PointsTeamSheetPlayerMap)
            {
                var playerTeamEntryCanvas = pair1.Value;
                var footballPlayerDetails = playerTeamEntryCanvas.GetComponent<FootballPlayerDetails>();

                foreach (var pair2 in GameweekAllPlayerPointsMap.Where(pair2 => pair2.Key == footballPlayerDetails.playerName))
                {
                    var playerGameweekPoints = pair2.Value[currentGwIndex];
                    if (!currentGwTeamSheetPointsMap.ContainsKey(footballPlayerDetails.playerName))
                    {
                        int.TryParse(playerGameweekPoints, out var gwPoints);
                        currentGwTeamSheetPointsMap.Add(footballPlayerDetails.playerName, gwPoints);
                    }
                    else
                        Debug.LogError("currentGwTeamSheetPointsMap already contains " + footballPlayerDetails.playerName);
                }
            }

            _gameweekTeamSheetPointsArray[currentGwIndex] = currentGwTeamSheetPointsMap;
        }

        private void SetHeadCoachPoints()
        {
            var headCoachDataObj = GameObjectFinder.FindSingleObjectByName("HeadCoachData");
            var headCoachData = headCoachDataObj.GetComponent<HeadCoachData>();
            
            headCoachData.coachPointsPerGw = new int[_noOfGameweeks];
            for (int i = 0; i < _noOfGameweeks; i++)
            {
                var totalGwPoints = 0;
                foreach (var pair in _gameweekTeamSheetPointsArray[i])
                {
                    totalGwPoints += pair.Value;        // need to account for subs bench!!
                }

                headCoachData.coachTotalPoints += totalGwPoints;
                headCoachData.coachPointsPerGw[i] = totalGwPoints;
            }
            
            headCoachData.UpdateHeadCoachSaveData();
        }
        
        private void SetPlayerPointsUi(int currentGwIndex)
        {
            foreach (var pair1 in PointsTeamSheetPlayerMap)
            {
                var playerTeamEntryCanvas = pair1.Value;
                var footballPlayerDetails = playerTeamEntryCanvas.GetComponent<FootballPlayerDetails>();
                var playerTeamEntryCanvasGrandChildren = GetGrandChildren(playerTeamEntryCanvas);
                var priceScoreObj = playerTeamEntryCanvasGrandChildren.Find(x => x.name == "PriceScore");

                foreach (var pair2 in GameweekAllPlayerPointsMap.Where(pair2 => pair2.Key == footballPlayerDetails.playerName))
                {
                    footballPlayerDetails.gameweekPoints = pair2.Value[currentGwIndex];
                }

                priceScoreObj.GetComponent<TMP_Text>().text = footballPlayerDetails.gameweekPoints;
            }
        }

        private void SetGameweekTitleNo(string gameweekNo)
        {
            var title = GameObjectFinder.FindSingleObjectByName("TotalGameweekPointsTitle");
            title.GetComponent<TMP_Text>().text = "Gameweek " + gameweekNo + " Points";
        }

        public void GameweekPointsButtonRight()
        {
            _currentGameweekIndex = UpIndex(_currentGameweekIndex, _noOfGameweeks);
            SetPlayerPointsUi(_currentGameweekIndex);
            SetGameweekTitleNo(_currentGameweekIndex.ToString());
        }

        public void GameweekPointsButtonLeft()
        {
            _currentGameweekIndex = DownIndex(_currentGameweekIndex);
            SetPlayerPointsUi(_currentGameweekIndex);
            SetGameweekTitleNo(_currentGameweekIndex.ToString());
        }
        
        private int DownIndex(int index)
        {
            index--;

            if (index >= 0) 
                return index;
            
            index = 0;
            return index;

        }

        private int UpIndex(int index, int arrayCount)
        {
            index++;

            if (index <= arrayCount - 1)
                return index;

            index = arrayCount - 1;
            return index;
        }
        
        private List<GameObject> GetGrandChildren(GameObject canvas)
        {
            var grandchildren = new List<GameObject>();
            var panel = canvas.transform.GetChild(0);

            for (int i = 0; i < panel.childCount; i++)
            {
                var playerObj = panel.GetChild(i).gameObject;
                grandchildren.Add(playerObj);
            }

            return grandchildren;
        }

        private List<GameObject> GetGreatGrandChildren(GameObject canvas)
        {
            var grandchildren = new List<GameObject>();
            var panel = canvas.transform.GetChild(0).GetChild(0);

            for (int i = 0; i < panel.childCount; i++)
            {
                var playerObj = panel.GetChild(i).gameObject;
                grandchildren.Add(playerObj);
            }

            return grandchildren;
        }
    }
}