using System;
using System.Collections;
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

        public static bool LoadingInProgress;
        
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

        // /// <summary>
        // /// Sets PlayFab title ID settings
        // /// Checks player preferences for a previous login on device. If player has previously logged in on the device then login automatically.
        // /// If auto login fails then activate login panel.
        // /// </summary>
        public void Start()
        {
            LoadingInProgress = true;
            StartCoroutine(Loading());
            
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
                // If using mobile device, then try login with DeviceID, if Device ID is not registered, create account
                var requestIos = new LoginWithIOSDeviceIDRequest{ DeviceId = ReturnMobileId() , CreateAccount = true };
                PlayFabClientAPI.LoginWithIOSDeviceID(requestIos, OnLoginSuccess, ErrorCallback);
#endif
#if UNITY_ANDROID
                
                var requestAndroid = new LoginWithAndroidDeviceIDRequest { AndroidDeviceId = ReturnMobileId() , CreateAccount = true };
                PlayFabClientAPI.LoginWithAndroidDeviceID(requestAndroid, OnLoginSuccess, ErrorCallback);
#endif
                DashBoardManager.Instance.SetScreenActive(0);        // set login panel active
            }
            
            DashBoardManager.Instance.SetGameObjectActive(false, "ScreenSelector");
            TransferListEntry.Instance.InstantiateTeamSheet("Transfer");
            DashBoardManager.Instance.LoadTransferList();
        }

        /// <summary>
        /// Sets Loading Panel active while loading in progress.
        /// loadingInProgress set to false once configs have all been fetched. (RemoteConfigManager.InitialLoadingComplete())
        /// </summary>
        /// <returns></returns>
        private IEnumerator Loading()
        {
            while (LoadingInProgress)
            {
                DashBoardManager.Instance.SetScreenActive(7);
                yield return new WaitForSeconds(0.1f);
            }
            
            Debug.Log("Loading Complete!!");
            ActivateDashBoard();
        }
        
        private void ActivateDashBoard()
        {
            DashBoardManager.Instance.SetScreenActive(1);
            DashBoardManager.Instance.SetGameObjectActive(true, "ScreenSelector");
        }

        #region Login
        /// <summary>
        /// If login successful;
        /// Set player preferences.
        /// Start loading player data and files.
        /// </summary>
        /// <param name="result"></param>
        private void OnLoginSuccess(LoginResult result)
        {
            Debug.Log("Boom!! Successful API Login.");
            
            PlayerPrefs.SetString("EMAIL", _userEmail);
            PlayerPrefs.SetString("PASSWORD", _userPassword);

            EntityId = result.EntityToken.Entity.Id;
            EntityType = result.EntityToken.Entity.Type;
            
            StartCoroutine(TeamSheetDatabase.Instance.WaitForPlayFabLogin());    // wait for playfab player login and load player data
        }

        /// <summary>
        /// Generates PlayFab Error Report
        /// </summary>
        /// <param name="error"></param>
        public void ErrorCallback(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }

        /// <summary>
        /// If auto login fails, Register new user 
        /// </summary>
        /// <param name="error"></param>
        private void OnLoginFailure(PlayFabError error)
        {
            var registerRequest = new RegisterPlayFabUserRequest {Email = _userEmail, Password = _userPassword, Username = _userName};
            PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, ErrorCallback);
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

        #region On Value Changed functions

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

        #endregion

        // Login button
        public void OnClickLogin()
        {
            var request = new LoginWithEmailAddressRequest { Email = _userEmail, Password = _userPassword };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
        }

        // returns device ID
        private string ReturnMobileId()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            return deviceId;
        }

        // Open Add Login panel for creating new accounts
        public void OpenAddLogin()
        {
            DashBoardManager.Instance.SetScreenActive(7);
        }

        // Login button for AddLoginPanel
        // Adds playfab username/password auth to an existing account created via an anonymous auth method, e.g. automatic device ID login.
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

        #region Change Account Buttons

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

        #endregion
    }
}