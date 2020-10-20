using System;
using System.Collections;
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
        private static string _playFabDisplayName;
        
        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        public void GetAccountInfo()
        {
            StartCoroutine(WaitForPlayerInfo());
        }

        /// <summary>
        /// Retrieves players Account Information and Profile from PlayFab
        /// Sets players PlayFabID and DisplayName
        /// </summary>
        /// <returns></returns>
        private IEnumerator WaitForPlayerInfo()
        {
            PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), OnGetAccountInfoSuccess, PlayFabController.Instance.ErrorCallback);
            PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest(), OnGetProfile, PlayFabController.Instance.ErrorCallback);
            yield return new WaitForSeconds(0.5f);
            SetProfileCanvasUi(_playFabIdentity, _playFabDisplayName);
        }

        private void OnGetProfile(GetPlayerProfileResult result)
        {
            _playFabDisplayName = result.PlayerProfile.DisplayName;
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
            _playFabDisplayName = newDisplayName;
        }

        /// <summary>
        /// Updates players display name and resets player profile panel Ui
        /// </summary>
        public void ChangePlayerDisplayName()
        {
            PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest {DisplayName = _playFabDisplayName}, PlayFabController.Instance.OnDisplayNameUpdated, PlayFabController.Instance.ErrorCallback);
            SetProfileCanvasUi(_playFabIdentity, _playFabDisplayName);
        }

        public void OpenChangeUsernamePanel()
        {
            DashBoardManager.Instance.SetGameObjectActive(true, "UpdateUsernamePanel");
        }

        public void CloseChangeUsernamePanel()
        {
            DashBoardManager.Instance.SetGameObjectActive(false, "UpdateUsernamePanel");
        }
    }
}