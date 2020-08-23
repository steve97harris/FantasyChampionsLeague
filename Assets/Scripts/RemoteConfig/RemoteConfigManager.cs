﻿using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Unity.RemoteConfig;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    public class RemoteConfigManager : MonoBehaviour
    {
        public struct userAttributes {}
        
        public struct appAttributes {}
        
        public static readonly List<string> PlayerRemoteConfigKeysList = new List<string>();
        
        public void Awake()
        {
            ConfigManager.FetchCompleted += UpdatePlayerPoints;
            ConfigManager.FetchCompleted += UpdateGameweekTitle;
            ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
        }
        
        public void FetchFootballPlayerPoints()
        {
            Debug.LogError("fetch");
            ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
        }

        private void UpdateGameweekTitle(ConfigResponse obj)
        {
            var currentGameweek = ConfigManager.appConfig.GetString("CURRENT_GAMEWEEK");
            var gameweekTitleObj = GameObjectFinder.FindSingleObjectByName("TotalGameweekPointsTitle");
            gameweekTitleObj.GetComponent<TMP_Text>().text = "Gameweek " + currentGameweek;
        }

        private void UpdatePlayerPoints(ConfigResponse obj)
        {
            var footballPlayerGwPointsMap = new Dictionary<string,int>();
            for (int i = 0; i < PlayerRemoteConfigKeysList.Count; i++)
            {
                var gwPoints = ConfigManager.appConfig.GetInt(PlayerRemoteConfigKeysList[i]);
                footballPlayerGwPointsMap.Add(PlayerRemoteConfigKeysList[i],gwPoints);
            }
            
            var teamSheetDatabaseObj = GameObjectFinder.FindSingleObjectByName("TeamSheetDatabase");
            var teamDatabase = teamSheetDatabaseObj.GetComponent<TeamSheetDatabase>();
            var teamSheetSaveData = teamDatabase.GetSavedTeamSheet();

            var teamSheetDataMap = teamSheetSaveData.teamSheetData;
            foreach (var pair in teamSheetDataMap)
            {
                foreach (var pair2 in footballPlayerGwPointsMap.Where(pair2 => pair2.Key == pair.Value.RemoteConfigKey))
                {
                    pair.Value.TotalPoints = pair2.Value.ToString();
                    Debug.LogError(pair2.Key + ", " + pair2.Value.ToString());
                }
            }
            
            teamSheetSaveData = new TeamSheetSaveData()
            {
                teamSheetData = teamSheetDataMap
            };
            
            teamDatabase.UpdateTeamSheetUi(teamSheetSaveData, "PointsTeamSheet");
        }

        private void OnDestroy()
        {
            ConfigManager.FetchCompleted -= UpdatePlayerPoints;
            ConfigManager.FetchCompleted -= UpdateGameweekTitle;
        }
    }
}