﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using PlayFab;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dashboard
{
    public class TransferListEntry : MonoBehaviour
    {
        public static TransferListEntry Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void TransferListEntryButton()
        {
            var thisButtonObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            
            // get footballer data of player clicked 
            var playerName = GetGameObjectChildText(thisButtonObj, 0);

            var playerDetails = new AthleteStats();
            var playerRemoteKeyMap = TransferListWindow.PlayerRemoteKeyMap;
            foreach (var pair in playerRemoteKeyMap.Where(pair => pair.Value.PlayerName == playerName))
            {
                playerDetails = pair.Value;
            }
            
            // Save new player entry to json
            var teamSheetDatabaseObj = GameObjectFinder.FindSingleObjectByName("TeamSheetDatabase");
            var teamDatabase = teamSheetDatabaseObj.GetComponent<TeamSheetDatabase>();
            
            var playerTeamEntryClickedObj = TransferListInitializer.PlayerTeamEntryClickedObj;
            var playerTeamSheetEntryDetails = playerTeamEntryClickedObj.GetComponent<FootballPlayerDetails>();

            var athleteStats = new AthleteStats
            {
                PlayerName = playerName, 
                Price = playerDetails.Price, 
                Rating = playerDetails.Rating,
                Position = playerDetails.Position, 
                Team = playerDetails.Team,
                TeamSheetPosition = playerTeamSheetEntryDetails.teamSheetPosition,
                TotalPoints = playerDetails.TotalPoints,
                RemoteConfigKey = playerDetails.RemoteConfigKey
            };

            if (IsValidPlayerPosition(playerDetails.Position, playerTeamSheetEntryDetails.teamSheetPosition) && !PlayerAlreadyInTeam(playerName) && !ToManyPlayersFromSameClub(playerDetails.Team) &&playerName != "")
            {
                Debug.Log("Valid player, insert player into team...");
               
                teamDatabase.InsertPlayerEntry(athleteStats, athleteStats.TeamSheetPosition);

                InstantiateTeamSheet("Transfer");
                
                var teamSheetSaveData = PlayFabEntityFileManager.Instance.GetTeamSheetData();
                teamDatabase.SetTeamSheetUi(teamSheetSaveData, "TransferTeamSheet(Clone)");
                teamDatabase.SetTeamSheetUi(teamSheetSaveData, "PointsTeamSheet");
                
                DashBoardManager.Instance.SetGameObjectActive(true, "TransferTeamSheet(Clone)");
                DashBoardManager.Instance.SetGameObjectActive(true, "ScreenSelector");
                TransferListWindow.DestroyTransferList();
            }
            else
            {
                Debug.LogError("Invalid player choice...");
                StartCoroutine(DisplayInvalidPlayerPosition());
            }
        }
        
        public void InstantiateTeamSheet(string teamSheetName)
        {
            var teamSheetObj = Resources.Load<GameObject>("Prefabs/" + teamSheetName + "Panel/" + teamSheetName + "TeamSheet");
            var teamSheetParent = GameObjectFinder.FindSingleObjectByName(teamSheetName + "Panel");
            
            
            var newTeamSheet = Instantiate(teamSheetObj, teamSheetParent.transform);
            newTeamSheet.SetActive(true);
        }

        private string GetGameObjectChildText(GameObject obj, int childIndex)
        {
            return obj.transform.GetChild(childIndex).GetComponent<TMP_Text>().text;
        }

        private bool IsValidPlayerPosition(string playerPosition, string teamSheetPosition)
        {
            switch (playerPosition)
            {
                case "Defender":
                    if (teamSheetPosition == "Cb_L" || teamSheetPosition == "Cb_R" || teamSheetPosition == "Rb" ||
                        teamSheetPosition == "Lb" || teamSheetPosition.Contains("Sub"))
                        return true;
                    break;
                case "Midfielder":
                    if (teamSheetPosition == "Cm_L" || teamSheetPosition == "Cm_C" || teamSheetPosition == "Cm_R" || teamSheetPosition.Contains("Sub"))
                        return true;
                    break;
                case "Forward":
                    if (teamSheetPosition == "Fw_L" || teamSheetPosition == "Fw_C" || teamSheetPosition == "Fw_R" || teamSheetPosition.Contains("Sub"))
                        return true;
                    break;
                case "Goalkeeper":
                    if (teamSheetPosition == "Gk" || teamSheetPosition.Contains("Sub"))
                        return true;
                    break;
                case "":
                    Debug.LogError("playerPosition not set");
                    break;
            }

            return false;
        }

        private bool PlayerAlreadyInTeam(string playerName)
        {
            var teamSheet =  PlayFabEntityFileManager.Instance.GetTeamSheetData();
            
            if (teamSheet == null)
                return false;
            
            foreach (var pair in teamSheet.teamSheetData)
            {
                if (pair.Value.PlayerName == playerName) return true;
            }

            return false;
        }

        private bool ToManyPlayersFromSameClub(string newPlayerClub)
        {
            var teamSheetSaveData = PlayFabEntityFileManager.Instance.GetTeamSheetData();
            
            var clubCountMap = new Dictionary<string, int>();
            foreach (var athleteTeam in teamSheetSaveData.teamSheetData.Select(pair => pair.Value.Team))
            {
                if (clubCountMap.ContainsKey(athleteTeam))
                    clubCountMap[athleteTeam]++;
                else 
                    clubCountMap.Add(athleteTeam, 1);
            }

            return clubCountMap.Where(pair => pair.Key == newPlayerClub).Any(pair => pair.Value == 3);
        }

        private IEnumerator DisplayInvalidPlayerPosition()
        {
            var invalidPlayerPosText = GameObjectFinder.FindSingleObjectByName("InvalidPlayerText");
            invalidPlayerPosText.SetActive(true);
            yield return new WaitForSeconds(3);
            invalidPlayerPosText.SetActive(false);
        }
    }
}