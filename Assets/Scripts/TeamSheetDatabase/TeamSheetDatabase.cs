﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Dashboard;
using DefaultNamespace;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.AuthenticationModels;
using PlayFab.ClientModels;
using PlayFab.DataModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using EntityKey = PlayFab.DataModels.EntityKey;

namespace DefaultNamespace
{
    public class TeamSheetDatabase : MonoBehaviour
    {
        public static TeamSheetDatabase Instance;

        private static Dictionary<string, ObjectResult> _playFabTeamSheetSaveDataMap;
        
        private static int maxNumberOfEntries = 15;

        #region Event Functions

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
        
        /// <summary>
        /// Coroutine to wait for PlayFab auto login
        /// </summary>
        /// <returns></returns>
        public IEnumerator WaitForPlayFabLogin()
        {
            // Wait for playfab controller to log in player
            yield return new WaitForSeconds(1f);
            
            StartCoroutine(WaitForFiles());
        }

        /// <summary>
        /// Coroutine to load player files, wait for files to be loaded.
        /// Fetch Remote Configs.
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForFiles()
        {
            PlayFabEntityFileManager.Instance.LoadPlayFabPlayerFiles();
            
            yield return new WaitForSeconds(2);

            RemoteConfigManager.Instance.FetchConfigs();
        }

        #endregion
        
        #region TeamSheet Entry Functions

        /// <summary>
        /// Inserts new player into TeamSheetSaveData json file.
        /// Replaces current player in teamSheetPlayerPosition with new player entry.
        /// Gets TeamSheetSaveData.
        /// Inserts new playerEntry.
        /// Saves TeamSheetSaveData to entity's profile.
        /// </summary>
        /// <param name="playerEntry"></param>
        /// <param name="teamSheetPlayerPosition"></param>
        public void InsertPlayerEntry(AthleteStats playerEntry, string teamSheetPlayerPosition)
        {
            Debug.Log("Inserting player into json file..");
            
            // get TeamSheetSaveData from json
            var teamSheetSaveData = PlayFabEntityFileManager.Instance.GetTeamSheetData();
            
            if (teamSheetSaveData == null)
            {
                teamSheetSaveData = new TeamSheetSaveData();
            }

            Debug.Log("savedTeamsheetEntries: " + teamSheetSaveData);
            
            // add or replace selected player
            if (teamSheetSaveData.teamSheetData.Count < maxNumberOfEntries)
            {
                // don't really need maxNumber of entries ?
                if (!teamSheetSaveData.teamSheetData.ContainsKey(teamSheetPlayerPosition))
                {
                    teamSheetSaveData.teamSheetData.Add(teamSheetPlayerPosition, playerEntry);
                }
                else
                {
                    teamSheetSaveData.teamSheetData[teamSheetPlayerPosition] = playerEntry;
                }
            }

            if (teamSheetSaveData.teamSheetData.Count >= maxNumberOfEntries)
            {
                teamSheetSaveData.teamSheetData[teamSheetPlayerPosition] = playerEntry;
            }

            PlayFabEntityFileManager.Instance.SavePlayFabTeamSheetData(teamSheetSaveData);
        }
        
        #endregion

        #region Private Methods

        /// <summary>
        /// Sets either Transfer or Points TeamSheet with new teamSheetSaveData, depending on specified teamSheetObjName.
        /// </summary>
        /// <param name="teamSheetSaveData"></param>
        /// <param name="teamSheetObjName"></param>
        public void SetTeamSheetUi(TeamSheetSaveData teamSheetSaveData, string teamSheetObjName)
        {
            if (teamSheetSaveData == null)
            {
                Debug.LogError("teamSheetSaveData returned null");
                return;
            }
            
            var teamSheetDataMap = teamSheetSaveData.teamSheetData;
            
            Debug.Log("Setting TeamSheetUi: " + teamSheetObjName);

            // set TeamSheetUi
            var transferTeamSheet = GameObjectFinder.FindSingleObjectByName(teamSheetObjName);
            var panel = transferTeamSheet.transform.GetChild(0).GetChild(0);
            var playerTeamEntryCanvasesCount = panel.childCount;
            
            Sprite playerTeamLogo = null;
            var teamLogosObj = GameObjectFinder.FindSingleObjectByName("TeamLogos");
            var teamLogos = teamLogosObj.GetComponent<TeamLogos>().teamLogos;
            
            var teamLogoNames = new List<string>();
            for (int i = 0; i < teamLogos.Count; i++)
            {
                teamLogoNames.Add(teamLogos[i].name);
            }
            
            for (int i = 0; i < playerTeamEntryCanvasesCount; i++)
            {
                var playerTeamEntryCanvas = panel.GetChild(i).gameObject;
                
                if (playerTeamEntryCanvas.GetComponent<FootballPlayerDetails>() == null)
                    continue;
                
                var playerTeamSheetPosition = playerTeamEntryCanvas.GetComponent<FootballPlayerDetails>().teamSheetPosition;
                
                if (!teamSheetDataMap.ContainsKey(playerTeamSheetPosition))
                    continue;
                
                var athleteStats = teamSheetDataMap[playerTeamSheetPosition];
                
                SetFootballPlayerDetails(playerTeamEntryCanvas, athleteStats);

                var obj = playerTeamEntryCanvas.transform.GetChild(0);
                obj.transform.GetChild(4).GetComponent<TMP_Text>().text = athleteStats.PlayerName;
                
                switch (teamSheetObjName)
                {
                    case "TransferTeamSheet(Clone)":
                        obj.transform.GetChild(3).GetComponent<TMP_Text>().text = "$" + athleteStats.Price;
                        break;
                    case "PointsTeamSheet":
                        obj.transform.GetChild(3).GetComponent<TMP_Text>().text = athleteStats.TotalPoints;
                        break;
                }

                if (teamLogoNames.Contains(athleteStats.Team))
                {
                    var indexOfTeamName = teamLogoNames.IndexOf(athleteStats.Team);
                    playerTeamLogo = teamLogos[indexOfTeamName];
                }
                
                obj.transform.GetChild(2).GetComponent<Image>().sprite = playerTeamLogo;
                var entryImage = obj.transform.GetChild(2).GetComponent<Image>();
                var color = entryImage.color;
                color.a = 1f;
                entryImage.color = color;
                obj.transform.GetChild(2).gameObject.transform.GetChild(0).GetComponent<TMP_Text>()
                    .text = "";
            }
        }

        public void SetFootballPlayerDetails(GameObject playerTeamEntryCanvas, AthleteStats athleteStats)
        {
            var playerDetails = playerTeamEntryCanvas.GetComponent<FootballPlayerDetails>();
            
            playerDetails.playerName = athleteStats.PlayerName;
            playerDetails.team = athleteStats.Team;
            playerDetails.price = athleteStats.Price;
            playerDetails.rating = athleteStats.Rating;
            playerDetails.position = athleteStats.Position;
            playerDetails.totalPlayerPoints = athleteStats.TotalPoints;
            playerDetails.remoteConfigKey = athleteStats.RemoteConfigKey;
        }
        #endregion
    }
}