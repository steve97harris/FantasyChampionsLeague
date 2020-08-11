using System.Collections.Generic;
using System.IO;
using System.Linq;
using Dashboard;
using DefaultNamespace;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class TeamSheetDatabase : MonoBehaviour
    {
        private static int maxNumberOfEntries = 15;
        private static string JsonPath => $"{Application.persistentDataPath}/TeamSheetData.json";

        #region Event Functions

        private void Start()
        {
            var savedTeamSheetEntries = GetSavedTeamSheet();
            UpdateTeamSheetUi(savedTeamSheetEntries);
            SaveTeamSheet(savedTeamSheetEntries);
        }

        #endregion
        
        #region TeamSheet Entry Functions

        public void InsertPlayerEntry(JsonPlayerDetails playerEntry, string teamSheetPlayerPosition)
        {
            // get TeamSheetSaveData from json
            var savedTeamSheetEntries = GetSavedTeamSheet();
            
            // add or replace selected player
            if (savedTeamSheetEntries.teamSheetData.Count < maxNumberOfEntries)
            {
                if (!savedTeamSheetEntries.teamSheetData.ContainsKey(teamSheetPlayerPosition))
                {
                    savedTeamSheetEntries.teamSheetData.Add(teamSheetPlayerPosition, playerEntry);
                }
                else
                {
                    savedTeamSheetEntries.teamSheetData[teamSheetPlayerPosition] = playerEntry;
                }
            }

            if (savedTeamSheetEntries.teamSheetData.Count > maxNumberOfEntries)
            {
                savedTeamSheetEntries.teamSheetData[teamSheetPlayerPosition] = playerEntry;
            }

            SaveTeamSheet(savedTeamSheetEntries);
        }
        
        #endregion

        #region Private Methods

        private TeamSheetSaveData GetSavedTeamSheet()
        {
            if (!File.Exists(JsonPath))
            {
                Debug.LogError("TeamSheetData.json does not exist - creating new one");
                
                // create json file
                File.Create(JsonPath).Dispose();
                return new TeamSheetSaveData();
            }

            using (StreamReader stream = new StreamReader(JsonPath))
            {
                // convert json string to TeamSheetSaveData
                var json = stream.ReadToEnd();
                var teamSheetSaveData = JsonConvert.DeserializeObject<TeamSheetSaveData>(json);
                return teamSheetSaveData;
            }
        }

        private void SaveTeamSheet(TeamSheetSaveData teamSheetSaveData)
        {
            using (StreamWriter stream = new StreamWriter(JsonPath))
            {
                // convert TeamSheetSaveData to json string
                var j = JsonConvert.SerializeObject(teamSheetSaveData, Formatting.Indented);
                Debug.LogError("json String: " + j);
                
                stream.Write(j);
            }
        }

        private void UpdateTeamSheetUi(TeamSheetSaveData teamSheetSaveData)
        {
            var teamSheetDataMap = teamSheetSaveData.teamSheetData;
            
            // set TeamSheetUi
            var transferTeamSheet = GameObjectFinder.FindSingleObjectByName("TransferTeamSheet");
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
                
                var jsonPlayerDetails = teamSheetDataMap[playerTeamSheetPosition];
                
                SetFootballPlayerDetailsValues(playerTeamEntryCanvas, jsonPlayerDetails);

                var obj = playerTeamEntryCanvas.transform.GetChild(0);
                obj.transform.GetChild(4).GetComponent<TMP_Text>().text = jsonPlayerDetails.PlayerName;
                obj.transform.GetChild(3).GetComponent<TMP_Text>().text = jsonPlayerDetails.Price;

                if (teamLogoNames.Contains(jsonPlayerDetails.Team))
                {
                    var indexOfTeamName = teamLogoNames.IndexOf(jsonPlayerDetails.Team);
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

        private void SetFootballPlayerDetailsValues(GameObject playerTeamEntryCanvas, JsonPlayerDetails jsonPlayerDetails)
        {
            var playerDetails = playerTeamEntryCanvas.GetComponent<FootballPlayerDetails>();
            
            playerDetails.playerName = jsonPlayerDetails.PlayerName;
            playerDetails.team = jsonPlayerDetails.Team;
            playerDetails.price = jsonPlayerDetails.Price;
            playerDetails.rating = jsonPlayerDetails.Rating;
            playerDetails.position = jsonPlayerDetails.Position;
        }
        #endregion
    }
}