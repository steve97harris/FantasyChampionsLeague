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
                Position = playerPosition, 
                Team = playerTeamLogo.name,
                TeamSheetPosition = playerDetails.teamSheetPosition
            };
            
            teamDatabase.InsertPlayerEntry(athleteStats, athleteStats.TeamSheetPosition);
            var teamSheetSaveData = teamDatabase.GetSavedTeamSheet();
            teamDatabase.UpdateTeamSheetUi(teamSheetSaveData, "TransferTeamSheet");
            teamDatabase.UpdateTeamSheetUi(teamSheetSaveData, "PointsTeamSheet");

            var list = GameObjectFinder.FindSingleObjectByName("TransferList(Clone)");
            var transferTeamSheet = GameObjectFinder.FindSingleObjectByName("TransferTeamSheet");
            
            list.SetActive(false);
            transferTeamSheet.SetActive(true);
            DestroyImmediate(list,true);
        }

        private string GetGameObjectChildText(GameObject obj, int childIndex)
        {
            return obj.transform.GetChild(childIndex).GetComponent<TMP_Text>().text;
        }
    }
}