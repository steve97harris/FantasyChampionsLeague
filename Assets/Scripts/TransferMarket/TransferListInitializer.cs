using Dashboard;
using UnityEngine;

namespace DefaultNamespace
{
    public class TransferListInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject transferListObj;
        [SerializeField] private GameObject transferTeamSheetObj;

        public static GameObject PlayerTeamEntryClickedObj;
        public void TeamSheetPlayerButton()
        {
            var transfersPage = GameObjectFinder.FindSingleObjectByName("TransfersPage");
            var pointsPage = GameObjectFinder.FindSingleObjectByName("PointsPage");

            if (transfersPage.activeInHierarchy)
            {
                var transferList = Instantiate(transferListObj, transfersPage.transform);
                transferList.SetActive(true);
            
                TransferListWindow.InitializePlayerList(TransferListWindow.PlayerPricesMap);
            
                transferTeamSheetObj.SetActive(false);
            
                var thisButtonObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
                var panel = thisButtonObj.transform.parent.gameObject;
                PlayerTeamEntryClickedObj = panel.transform.parent.gameObject;
            }

            if (pointsPage.activeInHierarchy)
            {
                // show player details
            }
        }
    }
}