using Dashboard;
using UnityEngine;

namespace DefaultNamespace
{
    public class TransferListInitializer : MonoBehaviour
    {
        [SerializeField] private GameObject transferListObj;

        public static GameObject PlayerTeamEntryObjClicked;
        
        /// <summary>
        /// TeamSheet button used to instantiate transfer list.
        /// </summary>
        public void TeamSheetPlayerButton()
        {
            // if button clicked on points team sheet then do nothing
            var transfersPage = GameObjectFinder.FindSingleObjectByName("TransferPanel");
            if (!transfersPage.activeInHierarchy) 
                return;
            
            var transferList = Instantiate(transferListObj, transfersPage.transform);
            transferList.SetActive(true);
            
            TransferListWindow.Instance.InitializePlayerList();
            
            DashBoardManager.Instance.SetGameObjectActive(false, "TransferTeamSheet(Clone)");
            
            var thisButtonObj = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
            var panel = thisButtonObj.transform.parent.gameObject;
            PlayerTeamEntryObjClicked = panel.transform.parent.gameObject;
        }
    }
}