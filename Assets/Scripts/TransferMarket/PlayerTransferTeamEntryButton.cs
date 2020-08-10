using Dashboard;
using UnityEngine;

namespace DefaultNamespace
{
    public class PlayerTransferTeamEntryButton : MonoBehaviour
    {
        [SerializeField] private GameObject transferListObj;
        [SerializeField] private GameObject transferTeamSheetObj;

        public static GameObject PlayerTeamEntryClickedObj;
        public void AddPlayerButtonClicked()
        {
            var transfersPage = GameObjectFinder.FindSingleObjectByName("TransfersPage");
            
            var transferList = Instantiate(transferListObj, transfersPage.transform);
            transferList.SetActive(true);
            PlayerTransferListWindow.InitializePlayerTransferList(PlayerTransferListWindow.PlayerPricesMap);
            transferTeamSheetObj.SetActive(false);
            
            var thisButtonObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            var panel = thisButtonObj.transform.parent.gameObject;
            PlayerTeamEntryClickedObj = panel.transform.parent.gameObject;
            Debug.LogError(PlayerTeamEntryClickedObj.name);
        }
    }
}