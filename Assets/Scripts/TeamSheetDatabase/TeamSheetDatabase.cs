using System;
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
        public static string JsonPath => Path.Combine(Application.persistentDataPath,"TeamSheetData.json");

        #region Event Functions

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            
            StartCoroutine(WaitForPlayFabLogin());
        }
        
        private IEnumerator WaitForPlayFabLogin()
        {
            yield return new WaitForSeconds(1f);
            LoadPlayFabFiles_SetTeamSheetUi();
        }

        private void LoadPlayFabFiles_SetTeamSheetUi()
        {
            StartCoroutine(WaitForFiles());
        }

        private IEnumerator WaitForFiles()
        {
            PlayFabEntityFileManager.Instance.LoadPlayFabPlayerFiles();
            
            yield return new WaitForSeconds(3f);
            
            var teamSheetSaveData = PlayFabEntityFileManager.Instance.GetTeamSheetData();
            
            Debug.LogError(teamSheetSaveData.teamSheetData.Count);
            SetTeamSheetUi(teamSheetSaveData, "TransferTeamSheet");
            
            RemoteConfigManager.Instance.FetchConfigs();
        }

        #endregion
        
        #region TeamSheet Entry Functions

        public void InsertPlayerEntry(AthleteStats playerEntry, string teamSheetPlayerPosition)
        {
            Debug.LogError("Inserting player into json file..");
            
            // get TeamSheetSaveData from json
            var teamSheetSaveData = PlayFabEntityFileManager.Instance.GetTeamSheetData();
            
            if (teamSheetSaveData == null)
            {
                teamSheetSaveData = new TeamSheetSaveData();
            }

            Debug.LogError("savedTeamsheetEntries: " + teamSheetSaveData);
            
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

        // public TeamSheetSaveData GetDeviceSavedTeamSheet()        // needs converting to playfab entity files
        // {
        //     if (!File.Exists(JsonPath))
        //     {
        //         Debug.LogError("TeamSheetData.json does not exist - creating new one");
        //         
        //         // create json file
        //         File.Create(JsonPath).Dispose();
        //         return new TeamSheetSaveData();
        //     }
        //     
        //     using (StreamReader stream = new StreamReader(JsonPath))
        //     {
        //         // convert json string to TeamSheetSaveData
        //         var json = stream.ReadToEnd();
        //         var teamSheetSaveData = JsonConvert.DeserializeObject<TeamSheetSaveData>(json);
        //         
        //         return teamSheetSaveData;
        //     }
        // }    

        // public void SaveDeviceTeamSheet(TeamSheetSaveData teamSheetSaveData)         // needs converting to playfab entity files
        // {
        //     using (StreamWriter stream = new StreamWriter(JsonPath))
        //     {
        //         // convert TeamSheetSaveData to json string
        //         var teamSheetSaveDataJson = JsonConvert.SerializeObject(teamSheetSaveData, Formatting.Indented);
        //         // Debug.Log("TeamSheetData (json String): " + teamSheetSaveDataJson);
        //         
        //         stream.Write(teamSheetSaveDataJson);
        //     }
        // }

        public void SetTeamSheetUi(TeamSheetSaveData teamSheetSaveData, string teamSheetObjName)
        {
            if (teamSheetSaveData == null)
            {
                Debug.LogError("SetTeamSheetUi: teamSheetSaveData returned NULL");
                return;
            }
            
            var teamSheetDataMap = teamSheetSaveData.teamSheetData;
            
            Debug.LogError(teamSheetObjName);

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
                
                SetFootballPlayerDetailsValues(playerTeamEntryCanvas, athleteStats);

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

        public static void SetFootballPlayerDetailsValues(GameObject playerTeamEntryCanvas, AthleteStats athleteStats)
        {
            var playerDetails = playerTeamEntryCanvas.GetComponent<FootballPlayerDetails>();
            
            playerDetails.playerName = athleteStats.PlayerName;
            playerDetails.team = athleteStats.Team;
            playerDetails.price = athleteStats.Price;
            playerDetails.rating = athleteStats.Rating;
            playerDetails.position = athleteStats.Position;
            playerDetails.gameweekPoints = athleteStats.TotalPoints;
            playerDetails.remoteConfigKey = athleteStats.RemoteConfigKey;
        }
        #endregion
    }
}