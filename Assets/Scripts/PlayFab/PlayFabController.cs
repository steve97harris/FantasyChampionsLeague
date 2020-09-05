using System;
using System.Collections.Generic;
using Dashboard;
using DefaultNamespace;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Serialization;
using LoginResult = PlayFab.ClientModels.LoginResult;

namespace PlayFab
{
    public class PlayFabController : MonoBehaviour
    {
        public static PlayFabController Instance;
        private string _userEmail;
        private string _userPassword;
        private string _userName;

        public static string EntityId;
        public static string EntityType;
        
        public void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                if (Instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
            DontDestroyOnLoad(this.gameObject.transform.parent.gameObject);
        }

        public void Start()
        {
            DashBoardManager.Instance.SetScreenActive(6);        // activate loading panel
            
            if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
            {
                PlayFabSettings.TitleId = "90D7E";
            }
            
            if (PlayerPrefs.HasKey("EMAIL"))
            {
                _userEmail = PlayerPrefs.GetString("EMAIL");
                _userPassword = PlayerPrefs.GetString("PASSWORD");
                _userName = PlayerPrefs.GetString("USERNAME");
                
                var request = new LoginWithEmailAddressRequest { Email = _userEmail, Password = _userPassword };
                PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
            }
            else
            {
#if UNITY_IOS
                var requestIos = new LoginWithIOSDeviceIDRequest{ DeviceId = ReturnMobileId() , CreateAccount = true };
                PlayFabClientAPI.LoginWithIOSDeviceID(requestIos, OnLoginSuccess, ErrorCallback);
#endif
#if UNITY_ANDROID
                
                var requestAndroid = new LoginWithAndroidDeviceIDRequest { AndroidDeviceId = ReturnMobileId() , CreateAccount = true };
                PlayFabClientAPI.LoginWithAndroidDeviceID(requestAndroid, OnLoginSuccess, ErrorCallback);
#endif
                
                // if automatic login fails, activate login panel
                DashBoardManager.Instance.SetScreenActive(0);        
            }

            DashBoardManager.Instance.SetGameObjectActive(false, "ScreenSelector");
            TransferListEntry.Instance.InstantiateTeamSheet("Transfer");
            DashBoardManager.Instance.LoadTransferList();
        }

        #region Login
        private void OnLoginSuccess(LoginResult result)
        {
            Debug.Log("Boom!! Successful API Login.");
            
            PlayerPrefs.SetString("EMAIL", _userEmail);
            PlayerPrefs.SetString("PASSWORD", _userPassword);

            EntityId = result.EntityToken.Entity.Id;
            EntityType = result.EntityToken.Entity.Type;
            
            StartCoroutine(TeamSheetDatabase.Instance.WaitForPlayFabLogin());    // wait for playfab player login and load player data
        }

        private void OnRegisterSuccess(RegisterPlayFabUserResult result)
        {
            Debug.Log("Boom!! Successful API Registration.");
            
            PlayerPrefs.SetString("EMAIL", _userEmail);
            PlayerPrefs.SetString("PASSWORD", _userPassword);

            PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest {DisplayName = _userName}, OnDisplayName, ErrorCallback);
        }

        public void OnDisplayName(UpdateUserTitleDisplayNameResult result)
        {
            Debug.Log("username: " + result.DisplayName);
        }

        public void ErrorCallback(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }

        private void OnLoginFailure(PlayFabError error)
        {
            var registerRequest = new RegisterPlayFabUserRequest {Email = _userEmail, Password = _userPassword, Username = _userName};
            PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, ErrorCallback);
        }

        public void GetUserEmail(string email)
        {
            _userEmail = email;
        }

        public void GetUserPassword(string password)
        {
            _userPassword = password;
        }

        public void GetUserName(string username)
        {
            _userName = username;
        }

        public void OnClickLogin()
        {
            var request = new LoginWithEmailAddressRequest { Email = _userEmail, Password = _userPassword };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
        }

        public static string ReturnMobileId()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            return deviceId;
        }

        public void OpenAddLogin()
        {
            DashBoardManager.Instance.SetScreenActive(7);
        }

        public void OnClickAddLogin()
        {
            var addLoginRequest = new AddUsernamePasswordRequest() {Email = _userEmail, Password = _userPassword, Username = _userName};
            PlayFabClientAPI.AddUsernamePassword(addLoginRequest, OnAddLoginSuccess, ErrorCallback);
        }
        
        private void OnAddLoginSuccess(AddUsernamePasswordResult result)
        {
            Debug.Log("Boom!! Successful API Registration.");
            
            PlayerPrefs.SetString("EMAIL", _userEmail);
            PlayerPrefs.SetString("PASSWORD", _userPassword);
        }

        public void ChangeAccountButton()
        {
            DashBoardManager.Instance.SetGameObjectActive(true, "ChangeAccountConfirmationPanel");
        }

        public void ChangeAccountYes()
        {
            DashBoardManager.Instance.SetScreenActive(0);
            DashBoardManager.Instance.SetGameObjectActive(false, "ScreenSelector");
        }

        public void ChangeAccountNo()
        {
            DashBoardManager.Instance.SetGameObjectActive(false, "ChangeAccountConfirmationPanel");
        }

        #endregion
    }
}