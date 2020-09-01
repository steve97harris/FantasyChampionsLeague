using System;
using System.Collections.Generic;
using DefaultNamespace;
using PlayFab.ClientModels;
using UnityEngine;
using LoginResult = PlayFab.ClientModels.LoginResult;

namespace PlayFab
{
    public class PlayFabController : MonoBehaviour
    {
        public static PlayFabController Instance;
        private string _userEmail;
        private string _userPassword;
        private string _userName;

        private static GameObject _loginPanel;
        private static GameObject _dashboard;
        private static GameObject _addLoginPanel;

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
            _loginPanel = GameObjectFinder.FindSingleObjectByName("LoginPanel");
            _dashboard = GameObjectFinder.FindSingleObjectByName("DashBoard");
            _addLoginPanel = GameObjectFinder.FindSingleObjectByName("AddLoginPanel");
            
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
            }
        }

        #region Login
        private void OnLoginSuccess(LoginResult result)
        {
            Debug.Log("Boom!! Successful API Login.");
            
            PlayerPrefs.SetString("EMAIL", _userEmail);
            PlayerPrefs.SetString("PASSWORD", _userPassword);

            EntityId = result.EntityToken.Entity.Id;
            EntityType = result.EntityToken.Entity.Type;
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
            _addLoginPanel.SetActive(true);
            _loginPanel.SetActive(false);
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
        #endregion
    }
}