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
            
            // set transfer teamsheet UI
            var playerTeamEntryClickedObj = TransferListInitializer.PlayerTeamEntryClickedObj;
            var playerDetails = playerTeamEntryClickedObj.GetComponent<FootballPlayerDetails>();
            playerDetails.playerName = playerName;
            playerDetails.price = playerPrice;
            playerDetails.position = playerPosition;
            playerDetails.team = playerTeamLogo.name;

            var obj = playerTeamEntryClickedObj.transform.GetChild(0);
            obj.transform.GetChild(4).GetComponent<TMP_Text>().text = playerName;
            obj.transform.GetChild(3).GetComponent<TMP_Text>().text = playerPrice;
            obj.transform.GetChild(2).GetComponent<Image>().sprite = playerTeamLogo;
            var entryImage = obj.transform.GetChild(2).GetComponent<Image>();
            var color = entryImage.color;
            color.a = 1f;
            entryImage.color = color;
            obj.transform.GetChild(2).gameObject.transform.GetChild(0).GetComponent<TMP_Text>()
                .text = "";
            
            var list = GameObjectFinder.FindSingleObjectByName("TransferList(Clone)");
            var transferTeamSheet = GameObjectFinder.FindSingleObjectByName("TransferTeamSheet");
            
            list.SetActive(false);
            transferTeamSheet.SetActive(true);
            DestroyImmediate(list,true);
            
            // Save new player entry to json
            var teamSheetDatabaseObj = GameObjectFinder.FindSingleObjectByName("TeamSheetDatabase");
            var teamDatabase = teamSheetDatabaseObj.GetComponent<TeamSheetDatabase>();

            var jsonPlayerDetails = new JsonPlayerDetails
            {
                PlayerName = playerName, 
                Price = playerPrice, 
                Position = playerPosition, 
                Team = playerTeamLogo.name,
                TeamSheetPosition = playerDetails.teamSheetPosition
            };

            teamDatabase.InsertPlayerEntry(jsonPlayerDetails, jsonPlayerDetails.TeamSheetPosition);
        }

        private string GetGameObjectChildText(GameObject obj, int childIndex)
        {
            return obj.transform.GetChild(childIndex).GetComponent<TMP_Text>().text;
        }
    }
}