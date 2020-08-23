using System.Collections;
using System.Linq;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dashboard
{
    public class TransferListEntry : MonoBehaviour
    {
        public void TransferListEntryButton()
        {
            var thisButtonObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            
            // footballer data of player clicked 
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

            if (IsValidPlayerPosition(playerDetails.Position, playerTeamSheetEntryDetails.teamSheetPosition) && !PlayerAlreadyInTeam(teamDatabase, playerName) || playerName == "")
            {
                Debug.LogError("Valid player, insert player into team...");
                InsertPlayerUpdateTeamSheetUi(teamDatabase, athleteStats);
                TransferListWindow.DestroyTransferList();
                InstantiateTransferTeamSheet(teamDatabase);
            }
            else
            {
                Debug.LogError("Invalid player choice...");
                StartCoroutine(DisplayInvalidPlayerPosition());
            }
        }

        private void InsertPlayerUpdateTeamSheetUi(TeamSheetDatabase teamDatabase, AthleteStats athleteStats)
        {
            teamDatabase.InsertPlayerEntry(athleteStats, athleteStats.TeamSheetPosition);
            var teamSheetSaveData = teamDatabase.GetSavedTeamSheet();
            teamDatabase.UpdateTeamSheetUi(teamSheetSaveData, "TransferTeamSheet");
            teamDatabase.UpdateTeamSheetUi(teamSheetSaveData, "PointsTeamSheet");
        }

        public void InstantiateTransferTeamSheet(TeamSheetDatabase teamDatabase)
        {
            var transferTeamSheetObj = Resources.Load<GameObject>("TransferTeamSheet");
            var transferPage = GameObjectFinder.FindSingleObjectByName("TransfersPage");
            var newTransferTeamSheet = Instantiate(transferTeamSheetObj, transferPage.transform);
            var teamSheetSaveData = teamDatabase.GetSavedTeamSheet();
            teamDatabase.UpdateTeamSheetUi(teamSheetSaveData, "TransferTeamSheet(Clone)");
            newTransferTeamSheet.SetActive(true);
            Debug.LogError(newTransferTeamSheet.activeInHierarchy);
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

        private bool PlayerAlreadyInTeam(TeamSheetDatabase teamSheetDatabase, string playerName)
        {
            var teamSheet = teamSheetDatabase.GetSavedTeamSheet();
            
            if (teamSheet == null)
                return false;
            
            foreach (var pair in teamSheet.teamSheetData)
            {
                if (pair.Value.PlayerName == playerName) return true;
            }

            return false;
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