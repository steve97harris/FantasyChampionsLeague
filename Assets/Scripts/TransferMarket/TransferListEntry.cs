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
            var playerPrice = GetGameObjectChildText(thisButtonObj, 1);
            var playerPosition = GetGameObjectChildText(thisButtonObj, 3);
            var playerTeamLogo = thisButtonObj.transform.GetChild(2).GetComponent<Image>().sprite;
            
            // Save new player entry to json
            var teamSheetDatabaseObj = GameObjectFinder.FindSingleObjectByName("TeamSheetDatabase");
            var teamDatabase = teamSheetDatabaseObj.GetComponent<TeamSheetDatabase>();
            
            var playerTeamEntryClickedObj = TransferListInitializer.PlayerTeamEntryClickedObj;
            var playerDetails = playerTeamEntryClickedObj.GetComponent<FootballPlayerDetails>();

            var athleteStats = new AthleteStats
            {
                PlayerName = playerName, 
                Price = playerPrice, 
                Rating = playerPrice,
                Position = playerPosition, 
                Team = playerTeamLogo.name,
                TeamSheetPosition = playerDetails.teamSheetPosition,
                Points = "0"
            };

            if (IsValidPlayerPosition(playerPosition, playerDetails.teamSheetPosition) && !PlayerAlreadyInTeam(teamDatabase, playerName) || playerName == "")
            {
                teamDatabase.InsertPlayerEntry(athleteStats, athleteStats.TeamSheetPosition);
                var teamSheetSaveData = teamDatabase.GetSavedTeamSheet();
                teamDatabase.UpdateTeamSheetUi(teamSheetSaveData, "TransferTeamSheet");
                teamDatabase.UpdateTeamSheetUi(teamSheetSaveData, "PointsTeamSheet");
                
                TransferListWindow.DestroyTransferList_LoadTransferTeamSheet();
            }
            else
            {
                StartCoroutine(DisplayInvalidPlayerPosition());
            }
        }

        private string GetGameObjectChildText(GameObject obj, int childIndex)
        {
            return obj.transform.GetChild(childIndex).GetComponent<TMP_Text>().text;
        }

        private bool IsValidPlayerPosition(string playerPosition, string teamSheetPosition)
        {
            switch (playerPosition)
            {
                case "D":
                    if (teamSheetPosition == "Cb_L" || teamSheetPosition == "Cb_R" || teamSheetPosition == "Rb" ||
                        teamSheetPosition == "Lb" || teamSheetPosition.Contains("Sub"))
                        return true;
                    break;
                case "M":
                    if (teamSheetPosition == "Cm_L" || teamSheetPosition == "Cm_C" || teamSheetPosition == "Cm_R" || teamSheetPosition.Contains("Sub"))
                        return true;
                    break;
                case "F":
                    if (teamSheetPosition == "Fw_L" || teamSheetPosition == "Fw_C" || teamSheetPosition == "Fw_R" || teamSheetPosition.Contains("Sub"))
                        return true;
                    break;
                case "Gk":
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