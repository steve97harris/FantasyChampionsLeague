using Dashboard;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerTransferTeamEntryButton : MonoBehaviour
    {
        [SerializeField] private GameObject transferListObj;
        [SerializeField] private GameObject transferTeamSheetObj;

        public static GameObject playerTeamEntryClickedObj;
        public void AddPlayerButtonClicked()
        {
            var transfersPage = GameObjectFinder.FindSingleObjectByName("TransfersPage");
            
            var transferList = Instantiate(transferListObj, transfersPage.transform);
            transferList.SetActive(true);
            PlayerTransferListWindow.InitializePlayerTransferList(PlayerTransferListWindow.PlayerPricesMap);
            transferTeamSheetObj.SetActive(false);
            
            var thisButtonObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            playerTeamEntryClickedObj = thisButtonObj.transform.parent.gameObject;
            Debug.LogError(playerTeamEntryClickedObj.name);
        }
    }
}