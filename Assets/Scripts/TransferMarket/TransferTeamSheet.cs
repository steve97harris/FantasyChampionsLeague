using UnityEngine;

namespace Dashboard
{
    public class TransferTeamSheet : MonoBehaviour
    {
        [SerializeField] private GameObject transferListObj;
        [SerializeField] private GameObject transferTeamSheetObj;
        
        public void AddPlayerButtonClicked(string pos)
        {
            var playerTransferEntryMap = TransferList.PlayerTransferEntryMap;
            
            
            transferListObj.SetActive(true);
            transferTeamSheetObj.SetActive(false);
        }
    }
}