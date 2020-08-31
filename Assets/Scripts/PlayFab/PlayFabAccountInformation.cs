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

        private static string _playFabIdentity;
        private static string _playerDisplayName;

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void GetAccountInfo() 
        {
            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), OnGetAccountInfoSuccess, PlayFabController.Instance.ErrorCallback);
            PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest(), OnGetProfile, PlayFabController.Instance.ErrorCallback);
            SetProfileCanvasUi(_playFabIdentity, _playerDisplayName);
        }

        private void OnGetProfile(GetPlayerProfileResult result)
        {
            _playerDisplayName = result.PlayerProfile.DisplayName;
        }
        
        private void OnGetAccountInfoSuccess(GetAccountInfoResult result)
        {
            _playFabIdentity = result.AccountInfo.PlayFabId;
        }

        private void SetProfileCanvasUi(string playFabId, string playFabUsername)
        {
            var playFabIdObj = GameObjectFinder.FindSingleObjectByName("PlayFabID");
            var playFabUsernameObj = GameObjectFinder.FindSingleObjectByName("PlayFabUsername");
            playFabIdObj.GetComponent<TMP_Text>().text = "PlayFab ID: " + playFabId;
            playFabUsernameObj.GetComponent<TMP_Text>().text = "Username: " + playFabUsername;
        }

        public void GetNewDisplayName(string newDisplayName)
        {
            
        }
    }
}