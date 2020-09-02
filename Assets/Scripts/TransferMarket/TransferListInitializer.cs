using Dashboard;
using UnityEngine;

namespace DefaultNamespace
{
    public class TransferListInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject transferListObj;

        public static GameObject PlayerTeamEntryClickedObj;
        public void TeamSheetPlayerButton()
        {
            var transfersPage = GameObjectFinder.FindSingleObjectByName("TransferPanel");

            if (!transfersPage.activeInHierarchy) 
                return;
            
            var transferList = Instantiate(transferListObj, transfersPage.transform);
            transferList.SetActive(true);
            
            TransferListWindow.InitializePlayerList(TransferListWindow.PlayerPricesMap);
            
            DashBoardManager.Instance.SetGameObjectActive(false, "TransferTeamSheet(Clone)");
            
            var thisButtonObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            var panel = thisButtonObj.transform.parent.gameObject;
            PlayerTeamEntryClickedObj = panel.transform.parent.gameObject;
        }
    }
}