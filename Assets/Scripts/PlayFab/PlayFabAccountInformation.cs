using System;
using DefaultNamespace;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;

namespace PlayFab
{
    public class PlayFabAccountInformation : MonoBehaviour
    {
        public static PlayFabAccountInformation Instance;

        public static string PlayFabIdentity;

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void GetAccountInfo() 
        {
            var request = new GetAccountInfoRequest();
            PlayFabClientAPI.GetAccountInfo(request, OnGetAccountInfoSuccess, PlayFabController.Instance.ErrorCallback);
        }
        
        private void OnGetAccountInfoSuccess(GetAccountInfoResult result)
        {
            PlayFabIdentity = result.AccountInfo.PlayFabId;
            SetProfileCanvasUi(result.AccountInfo.PlayFabId, result.AccountInfo.Username);
        }

        private void SetProfileCanvasUi(string playFabId, string playFabUsername)
        {
            var playFabIdObj = GameObjectFinder.FindSingleObjectByName("PlayFabID");
            var playFabUsernameObj = GameObjectFinder.FindSingleObjectByName("PlayFabUsername");
            playFabIdObj.GetComponent<TMP_Text>().text = "PlayFab ID: " + playFabId;
            playFabUsernameObj.GetComponent<TMP_Text>().text = "Username: " + playFabUsername;
        }
    }
}