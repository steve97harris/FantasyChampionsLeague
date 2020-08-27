﻿using System;
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
        private struct UserAttributes {}

        private struct AppAttributes {}
        
        public static readonly List<string> PlayerRemoteConfigKeysList = new List<string>();

        public void Awake()
        {
            StartCoroutine(WaitForLogin());
        }

        private IEnumerator WaitForLogin()
        {
            yield return new WaitForSeconds(0.5f);
            FetchConfigs();
        }

        private void FetchConfigs()
        {
            ConfigManager.FetchConfigs<UserAttributes, AppAttributes>(new UserAttributes(), new AppAttributes());
            ConfigManager.FetchCompleted += UpdatePlayerPoints;
            ConfigManager.FetchCompleted += UpdateGameweekTitle;
        }
        
        public void FetchFootballPlayerPoints()
        {
            ConfigManager.FetchConfigs<UserAttributes, AppAttributes>(new UserAttributes(), new AppAttributes());
        }
        
        private void SetCoachUi()
        {
            var headCoachDataObj = GameObjectFinder.FindSingleObjectByName("HeadCoachData");
            var headCoachData = headCoachDataObj.GetComponent<HeadCoachData>();
            headCoachData.SetHeadCoachUi();
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
                }
            }
            
            teamSheetSaveData = new TeamSheetSaveData()
            {
                teamSheetData = teamSheetDataMap
            };
            
            teamDatabase.SetTeamSheetUi(teamSheetSaveData, "PointsTeamSheet");
            teamDatabase.SaveTeamSheet(teamSheetSaveData);
            
            PointsTeamSheetManager.SetHeadCoachPoints();
            SetCoachUi();
        }

        private void OnDestroy()
        {
            ConfigManager.FetchCompleted -= UpdatePlayerPoints;
            ConfigManager.FetchCompleted -= UpdateGameweekTitle;
        }
    }
}