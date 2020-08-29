using System;
using System.Collections.Generic;
using System.Linq;
using CSV;
using Dashboard;
using GoogleSheetsLevelSynchronizer;
using PlayFab;
using TMPro;
using Unity.RemoteConfig;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    public class PointsTeamSheetManager : MonoBehaviour
    {
        public static PointsTeamSheetManager Instance;
        private static int _noOfGameweeks = 35;
        
        /// <summary> PointsTeamSheetPlayerMap - map of player prefabs 
        /// <typeparam name="Key">Football Player Name</typeparam>
        /// <typeparam name="Value">Football Player Prefab</typeparam>
        /// </summary>
        private static readonly Dictionary<string, GameObject> PointsTeamSheetPlayerMap = new Dictionary<string, GameObject>();

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
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

        public void SetHeadCoachPoints()
        {
            GetPlayerMap();
            
            var currentGameweek = ConfigManager.appConfig.GetString("CURRENT_GAMEWEEK");

            var headCoachSaveData = PlayFabEntityFileManager.Instance.GetHeadCoachData();
            
            if (headCoachSaveData.CoachName == null)
            {
                headCoachSaveData.CoachName = "";
            }
            if (headCoachSaveData.ClubName == null)
            {
                headCoachSaveData.ClubName = "";
            }
            if (headCoachSaveData.CoachPointsPerGw == null)
            {
                headCoachSaveData.CoachPointsPerGw = new int[_noOfGameweeks];
            }
            
            for (int i = 0; i < headCoachSaveData.CoachPointsPerGw.Length; i++)
            {
                if (currentGameweek == (i + 1).ToString())
                {
                    // get all player gw points 
                    var totalGwPoints = 0;
                    foreach (var pair in PointsTeamSheetPlayerMap)
                    {
                        var gwPointsString = pair.Value.GetComponent<FootballPlayerDetails>().gameweekPoints;
                        int.TryParse(gwPointsString, out var gwPoints);
                        totalGwPoints += gwPoints;                            // need to account for subs bench!!
                    }

                    headCoachSaveData.CoachPointsPerGw[i] = totalGwPoints;
                    headCoachSaveData.CoachCurrentGwPoints = totalGwPoints;
                }
            }

            headCoachSaveData.CoachTotalPoints = 0;
            for (int i = 0; i < headCoachSaveData.CoachPointsPerGw.Length; i++)
            {
                headCoachSaveData.CoachTotalPoints += headCoachSaveData.CoachPointsPerGw[i];
            }

            PlayFabEntityFileManager.Instance.SavePlayFabHeadCoachSaveData(headCoachSaveData);
            
            
            PlayFabPlayerStats.Instance.SetStatistics();
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

        #region Unfinished Coach Points Per Gameweek Functions

        /*        
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
            */

        #endregion
    }
}