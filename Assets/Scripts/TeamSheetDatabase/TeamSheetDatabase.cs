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
            UpdateTeamSheetUiSaveTeamSheet();
        }

        private void UpdateTeamSheetUiSaveTeamSheet()
        {
            var savedTeamSheetEntries = GetSavedTeamSheet();
            UpdateTeamSheetUi(savedTeamSheetEntries, "TransferTeamSheet");
            UpdateTeamSheetUi(savedTeamSheetEntries, "PointsTeamSheet");
            SaveTeamSheet(savedTeamSheetEntries);
        }

        #endregion
        
        #region TeamSheet Entry Functions

        public void InsertPlayerEntry(AthleteStats playerEntry, string teamSheetPlayerPosition)
        {
            Debug.LogError("Inserting player into json file..");
            
            // get TeamSheetSaveData from json
            var savedTeamSheetEntries = GetSavedTeamSheet() ?? new TeamSheetSaveData();
            Debug.LogError("savedTeamsheetEntries: " + savedTeamSheetEntries);
            
            // add or replace selected player
            if (savedTeamSheetEntries.teamSheetData.Count < maxNumberOfEntries)
            {
                // don't really need maxNumber of entries ?
                if (!savedTeamSheetEntries.teamSheetData.ContainsKey(teamSheetPlayerPosition))
                {
                    savedTeamSheetEntries.teamSheetData.Add(teamSheetPlayerPosition, playerEntry);
                }
                else
                {
                    savedTeamSheetEntries.teamSheetData[teamSheetPlayerPosition] = playerEntry;
                }
            }

            if (savedTeamSheetEntries.teamSheetData.Count >= maxNumberOfEntries)
            {
                savedTeamSheetEntries.teamSheetData[teamSheetPlayerPosition] = playerEntry;
            }

            SaveTeamSheet(savedTeamSheetEntries);
        }
        
        #endregion

        #region Private Methods

        public TeamSheetSaveData GetSavedTeamSheet()
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

        public void SaveTeamSheet(TeamSheetSaveData teamSheetSaveData)
        {
            using (StreamWriter stream = new StreamWriter(JsonPath))
            {
                // convert TeamSheetSaveData to json string
                var j = JsonConvert.SerializeObject(teamSheetSaveData, Formatting.Indented);
                Debug.LogError("json String: " + j);
                
                stream.Write(j);
            }
        }

        public void UpdateTeamSheetUi(TeamSheetSaveData teamSheetSaveData, string teamSheetObjName)
        {
            Debug.LogError("updating teamsheet ui");
            if (teamSheetSaveData == null)
                return;
            
            var teamSheetDataMap = teamSheetSaveData.teamSheetData;
            
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
                    case "TransferTeamSheet":
                        obj.transform.GetChild(3).GetComponent<TMP_Text>().text = athleteStats.Price;
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