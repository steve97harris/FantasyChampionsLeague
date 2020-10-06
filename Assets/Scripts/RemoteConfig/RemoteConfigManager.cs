﻿using System;
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
            ConfigManager.FetchCompleted += SetPoints;
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
        private void SetPoints(ConfigResponse obj)
        {
            var teamSheetSaveData = PlayFabEntityFileManager.Instance.GetTeamSheetData();
            if (teamSheetSaveData.teamSheetData == null)
            {
                Debug.LogError("teamSheetData returned null");
                return;
            }

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
            ConfigManager.FetchCompleted -= SetTeamSheet;
            ConfigManager.FetchCompleted -= SetPoints;
            ConfigManager.FetchCompleted -= InitialLoadingComplete;
        }
    }
}