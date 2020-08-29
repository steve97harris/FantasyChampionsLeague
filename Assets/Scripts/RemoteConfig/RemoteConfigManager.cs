using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayFab;
using TMPro;
using UnityEngine;
using Unity.RemoteConfig;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    public class RemoteConfigManager : MonoBehaviour
    {
        public static RemoteConfigManager Instance;
        private struct UserAttributes {}

        private struct AppAttributes {}
        
        public static readonly List<string> PlayerRemoteConfigKeysList = new List<string>();

        public void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void FetchConfigs()
        {
            ConfigManager.FetchConfigs<UserAttributes, AppAttributes>(new UserAttributes(), new AppAttributes());
            ConfigManager.FetchCompleted += UpdatePlayerPoints;
            ConfigManager.FetchCompleted += UpdateGameweekTitle;
        }
        
        public void FetchFootballPlayerPoints()
        {
            ConfigManager.FetchConfigs<UserAttributes, AppAttributes>(new UserAttributes(), new AppAttributes());
        }

        /* Currently only updating player points on refresh button.
         * Changes to make:
         * {
         *  - Update Football Player Points with respect to config data :)
         *  - Save new TeamSheetSaveData to device and playfab :)
         *  - Set Points TeamSheet Ui with new TeamSheetSaveData :)
         *  - Add up new coach points
         *  - Create new HeadCoachSaveData
         *  - Save new HeadCoachSaveData to device
         *  - Save coach points to playfab player statistics
         *  - Set HeadCoachUi with new HeadCoachSaveData
         * }
         */
        private void UpdatePlayerPoints(ConfigResponse obj)
        {
            var footballPlayerGwPointsMap = new Dictionary<string,int>();
            for (int i = 0; i < PlayerRemoteConfigKeysList.Count; i++)
            {
                var gwPoints = ConfigManager.appConfig.GetInt(PlayerRemoteConfigKeysList[i]);
                footballPlayerGwPointsMap.Add(PlayerRemoteConfigKeysList[i],gwPoints);
            }
            
            var teamSheetSaveData = PlayFabEntityFileManager.Instance.GetTeamSheetData();
            if (teamSheetSaveData.teamSheetData == null)
            {
                Debug.LogError("playfab teamsheet NULL");
                return;
            } 

            var teamSheetDataMap = teamSheetSaveData.teamSheetData;
            foreach (var pair in teamSheetDataMap)
            {
                foreach (var pair2 in footballPlayerGwPointsMap.Where(pair2 => pair2.Key == pair.Value.RemoteConfigKey))
                {
                    pair.Value.TotalPoints = pair2.Value.ToString();
                }
            }
            
            teamSheetSaveData = new TeamSheetSaveData()
            {
                teamSheetData = teamSheetDataMap
            };
            
            PlayFabEntityFileManager.Instance.SavePlayFabTeamSheetData(teamSheetSaveData);
            
            var teamSheetDatabaseObj = GameObjectFinder.FindSingleObjectByName("TeamSheetDatabase");
            var teamDatabase = teamSheetDatabaseObj.GetComponent<TeamSheetDatabase>();
            teamDatabase.SetTeamSheetUi(teamSheetSaveData, "PointsTeamSheet");

            PointsTeamSheetManager.Instance.SetHeadCoachPoints();
            SetCoachUi();
        }
        
        private void SetCoachUi()
        {
            var totalCoachPoints = GameObjectFinder.FindSingleObjectByName("HeadCoachTotalPoints");
            var currentGwPoints = GameObjectFinder.FindSingleObjectByName("CurrentGameweekPoints");

            var coachTotalPoints = PlayFabPlayerStats.Instance.coachTotalPoints;
            var coachCurrentGwPoints = PlayFabPlayerStats.Instance.coachCurrentGwPoints;
            
            totalCoachPoints.GetComponent<TMP_Text>().text = coachTotalPoints.ToString();
            currentGwPoints.GetComponent<TMP_Text>().text = coachCurrentGwPoints.ToString();
            
            Debug.LogError("Coach Ui Data: { totalPoints: " + coachTotalPoints + " gwPoints: " + coachCurrentGwPoints + "}");
        }

        private void UpdateGameweekTitle(ConfigResponse obj)
        {
            var currentGameweek = ConfigManager.appConfig.GetString("CURRENT_GAMEWEEK");
            var gameweekTitleObj = GameObjectFinder.FindSingleObjectByName("TotalGameweekPointsTitle");
            gameweekTitleObj.GetComponent<TMP_Text>().text = "Gameweek " + currentGameweek;
        }

        private void OnDestroy()
        {
            ConfigManager.FetchCompleted -= UpdatePlayerPoints;
            ConfigManager.FetchCompleted -= UpdateGameweekTitle;
        }
    }
}