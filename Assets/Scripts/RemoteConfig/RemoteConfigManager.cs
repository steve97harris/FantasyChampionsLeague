using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
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
            ConfigManager.FetchCompleted += InitialLoadingComplete;
        }

        public void FetchFootballPlayerPoints()
        {
            ConfigManager.FetchConfigs<UserAttributes, AppAttributes>(new UserAttributes(), new AppAttributes());
        }

        /// <summary>
        /// Retrieves all footballers current gameweek points.
        /// Updates TeamSheetSaveData with current points.
        /// Saves TeamSheetSaveData to entity's playfab profile.
        /// Sets TeamSheetUi.
        /// Updates and Saves HeadCoachSaveData.
        /// Sets HeadCoachUi.
        /// Sets Gameweek Title.
        /// </summary>
        /// <param name="obj"></param>
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
                Debug.LogError("teamSheetData returned null");
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
            
            TeamSheetDatabase.Instance.SetTeamSheetUi(teamSheetSaveData, "PointsTeamSheet");

            PointsTeamSheetManager.Instance.SetHeadCoachPoints();
            PointsTeamSheetManager.Instance.SetHeadCoachUi();
            PointsTeamSheetManager.Instance.UpdateGameweekTitle();
        }

        /// <summary>
        /// Function to activate dashboard once all configs have been fetched.
        /// (Need better way of setting the loading screen?)
        /// </summary>
        /// <param name="obj"></param>
        private void InitialLoadingComplete(ConfigResponse obj)
        {
            /* Only effective if FetchConfigs is last function in TeamSheetDatabase Coroutines.
             * RemoteConfigManager.Instance.FetchConfigs();    (TeamSheetDatabase line 64) */

            PlayFabController.LoadingInProgress = false;
        }

        private void OnDestroy()
        {
            ConfigManager.FetchCompleted -= UpdatePlayerPoints;
            ConfigManager.FetchCompleted -= InitialLoadingComplete;
        }
    }
}