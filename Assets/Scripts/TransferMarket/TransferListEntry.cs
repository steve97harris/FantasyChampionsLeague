using System;
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

        /// <summary>
        /// Button to insert new footballer into a players team sheet from the transfer list.
        /// </summary>
        public void TransferListEntryButton()
        {
            var thisButtonObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            
            // Get unique remote config key of footballer selected
            var playerRemoteConfigKey =
                thisButtonObj.transform.parent.GetComponent<FootballPlayerDetails>().remoteConfigKey;

            // Get athlete stats from player remote key map using players name
            var athleteStats = new AthleteStats();
            var playerRemoteKeyMap = TransferListWindow.PlayerRemoteKeyMap;
            foreach (var pair in playerRemoteKeyMap.Where(pair => pair.Value.RemoteConfigKey == playerRemoteConfigKey))
            {
                athleteStats = pair.Value;
            }
            
            // Get the team sheet player that was selected to be replaced with new player
            var playerTeamEntryClickedObj = TransferListInitializer.PlayerTeamEntryObjClicked;
            var playerTeamSheetEntryDetails = playerTeamEntryClickedObj.GetComponent<FootballPlayerDetails>();
            
            // If valid player entry, insert selected footballer into team
            if (IsValidFootballerEntry(athleteStats, playerTeamSheetEntryDetails))
            {
                Debug.Log("Valid player, insert player into team...");
               
                TeamSheetDatabase.Instance.InsertPlayerEntry(athleteStats, playerTeamSheetEntryDetails.teamSheetPosition);

                InstantiateTeamSheet("Transfer");
                
                var teamSheetSaveData = PlayFabEntityFileManager.Instance.GetTeamSheetData();
                TeamSheetDatabase.Instance.SetTeamSheetUi(teamSheetSaveData, "TransferTeamSheet(Clone)");
                TeamSheetDatabase.Instance.SetTeamSheetUi(teamSheetSaveData, "PointsTeamSheet");
                
                DashBoardManager.Instance.SetGameObjectActive(true, "TransferTeamSheet(Clone)");
                DashBoardManager.Instance.SetGameObjectActive(true, "ScreenSelector");
                
                TransferListWindow.Instance.DestroyTransferList();
            }
            else
            {
                Debug.LogError("Invalid player choice...");
                StartCoroutine(DisplayInvalidPlayerPosition());
            }
        }

        /// <summary>
        /// Checks whether the selected footballer is valid for the players team sheet
        /// </summary>
        /// <param name="athleteStats"></param>
        /// <param name="playerTeamSheetEntryDetails"></param>
        /// <returns></returns>
        private bool IsValidFootballerEntry(AthleteStats athleteStats, FootballPlayerDetails playerTeamSheetEntryDetails)
        {
            var isValid = IsValidPlayerPosition(athleteStats.Position, playerTeamSheetEntryDetails.teamSheetPosition) &&
                           !PlayerAlreadyInTeam(athleteStats.Name) && !ToManyPlayersFromSameClub(athleteStats.Team) && athleteStats.Name != "";
            
            return isValid;
        }
        
        /// <summary>
        /// Instantiates either Transfer or Points TeamSheet 
        /// </summary>
        /// <param name="teamSheetName"></param>
        public void InstantiateTeamSheet(string teamSheetName)
        {
            var teamSheetObj = Resources.Load<GameObject>("Prefabs/" + teamSheetName + "Panel/" + teamSheetName + "TeamSheet");
            var teamSheetParent = GameObjectFinder.FindSingleObjectByName(teamSheetName + "Panel");

            var newTeamSheet = Instantiate(teamSheetObj, teamSheetParent.transform);
            newTeamSheet.SetActive(true);
        }

        /// <summary>
        /// Checks whether chosen player can be entered into chosen team sheet position
        /// </summary>
        /// <param name="playerPosition"></param>
        /// <param name="teamSheetPosition"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Checks to see if player selected is already in the team sheet
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns></returns>
        private bool PlayerAlreadyInTeam(string playerName)
        {
            var teamSheet =  PlayFabEntityFileManager.Instance.GetTeamSheetData();
            
            if (teamSheet == null)
                return false;
            
            foreach (var pair in teamSheet.teamSheetData)
            {
                if (pair.Value.Name == playerName) return true;
            }

            return false;
        }

        /// <summary>
        /// Check whether the limit of footballers from the same club is met.
        /// Limit of 3 players from the same club.
        /// </summary>
        /// <param name="newPlayerClub"></param>
        /// <returns></returns>
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

            foreach (var pair in clubCountMap)
            {
                if (pair.Key == newPlayerClub)
                {
                    if (pair.Value == 3) return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Display's Ui text when invalid player is selected.
        /// </summary>
        /// <returns></returns>
        private IEnumerator DisplayInvalidPlayerPosition()
        {
            var invalidPlayerText = GameObjectFinder.FindSingleObjectByName("InvalidPlayerText");
            invalidPlayerText.SetActive(true);
            yield return new WaitForSeconds(3);
            invalidPlayerText.SetActive(false);
        }
    }
}