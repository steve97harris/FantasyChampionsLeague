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
            ConfigManager.FetchCompleted += SetTeamSheet;
            ConfigManager.FetchCompleted += ConfigurePointsWithFootballPlayerPointsDatabase;
            ConfigManager.FetchCompleted += InitialLoadingComplete;
        }

        public void FetchFootballPlayerPoints()
        {
            ConfigManager.FetchConfigs<UserAttributes, AppAttributes>(new UserAttributes(), new AppAttributes());
        }

        /// <summary>
        /// Gets TeamSheetSaveData from entity's profile.
        /// Sets TeamSheetUi with TeamSheetSaveData.
        /// </summary>
        /// <param name="obj"></param>
        private void SetTeamSheet(ConfigResponse obj)
        {
            var teamSheetSaveData = PlayFabEntityFileManager.Instance.GetTeamSheetData();
            Debug.Log(teamSheetSaveData.teamSheetData.Count);

            TeamSheetDatabase.Instance.SetTeamSheetUi(teamSheetSaveData, "TransferTeamSheet(Clone)");
        }

        /// <summary>
        /// Sets TeamSheetUi.
        /// Updates and Saves HeadCoachSaveData.
        /// Sets HeadCoachUi.
        /// Sets Gameweek Title.
        /// </summary>
        /// <param name="obj"></param>
        private async void ConfigurePointsWithFootballPlayerPointsDatabase(ConfigResponse obj)
        {
            var teamSheetSaveData = PlayFabEntityFileManager.Instance.GetTeamSheetData();
            if (teamSheetSaveData.teamSheetData == null)
            {
                Debug.LogError("teamSheetData returned null");
                return;
            }

            var fileBStream = await FirebaseDataStorage.Instance.DownloadFileStreamAsync(DashBoardManager.FileNameB);
            var csvList = CsvReader.LoadCsvFileViaStream(fileBStream);

            var currentTeamSheetData = teamSheetSaveData.teamSheetData;
            foreach (var pair in currentTeamSheetData)
            {
                var athleteStats = pair.Value;
                var remoteConfigKey = athleteStats.RemoteConfigKey;

                var csvPlayerData = csvList.Select(x => x).Where(x => x.Contains(remoteConfigKey)).ToArray();
                if (csvPlayerData.Length != 1)
                {
                    Debug.LogError("Error: 2 players with same RemoteConfigKey. Continuing...");
                    continue;
                }

                var playerData = csvPlayerData[0];
                var playerDataSplit = playerData.Split(',');
                var currentPlayerPoints = playerDataSplit[2];
                
                athleteStats.TotalPoints = currentPlayerPoints;
                
                Debug.LogError("player data: " + playerData);
            }

            teamSheetSaveData.teamSheetData = currentTeamSheetData;

            TeamSheetDatabase.Instance.SetTeamSheetUi(teamSheetSaveData, "PointsTeamSheet");

            GamePlayerPointsModule.Instance.SetHeadCoachPoints();
            GamePlayerPointsModule.Instance.SetHeadCoachUi();
            GamePlayerPointsModule.Instance.UpdateGameweekTitle();
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

            DashBoardManager.Instance.ActivateDashBoard();
        }

        private void OnDestroy()
        {
            ConfigManager.FetchCompleted -= SetTeamSheet;
            ConfigManager.FetchCompleted -= ConfigurePointsWithFootballPlayerPointsDatabase;
            ConfigManager.FetchCompleted -= InitialLoadingComplete;
        }
    }
}