using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dashboard
{
    public class PlayerTransferListEntryButton : MonoBehaviour
    {
        public void PlayerTransferEntryButtonClicked()
        {
            var thisButtonObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            Debug.LogError(thisButtonObj.transform.GetChild(0).GetComponent<TMP_Text>().text);
            
            
            // footballer data of player clicked 
            var playerName = GetGameObjectChildText(thisButtonObj, 0);
            var playerPrice = GetGameObjectChildText(thisButtonObj, 1);
            var playerPosition = GetGameObjectChildText(thisButtonObj, 3);
            var playerTeamLogo = thisButtonObj.transform.GetChild(2).GetComponent<Image>().sprite;
            
            // sets transfer team page
            // need to create json to store details
            var playerTeamEntryClickedObj = PlayerTransferTeamEntryButton.playerTeamEntryClickedObj;
            var playerDetails = playerTeamEntryClickedObj.GetComponent<FootballPlayerDetails>();
            playerDetails.playerName = playerName;
            playerDetails.price = playerPrice;
            playerDetails.position = playerPosition;
            playerDetails.team = playerTeamLogo.name;

            playerTeamEntryClickedObj.transform.GetChild(4).GetComponent<TMP_Text>().text = playerName;
            playerTeamEntryClickedObj.transform.GetChild(3).GetComponent<TMP_Text>().text = playerPrice;
            playerTeamEntryClickedObj.transform.GetChild(2).GetComponent<Image>().sprite = playerTeamLogo;
            var entryImage = playerTeamEntryClickedObj.transform.GetChild(2).GetComponent<Image>();
            var color = entryImage.color;
            color.a = 1f;
            entryImage.color = color;
            playerTeamEntryClickedObj.transform.GetChild(2).gameObject.transform.GetChild(0).GetComponent<TMP_Text>()
                .text = "";
            
            var list = GameObjectFinder.FindSingleObjectByName("TransferList(Clone)");
            var transferTeamSheet = GameObjectFinder.FindSingleObjectByName("TransferTeamSheet");
            Debug.LogError(transferTeamSheet.name);
            
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