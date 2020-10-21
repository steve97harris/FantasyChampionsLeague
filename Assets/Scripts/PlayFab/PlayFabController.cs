using System;
using System.Collections;
using System.Collections.Generic;
using Dashboard;
using DefaultNamespace;
using PlayFab.ClientModels;
using TMPro;
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
            DashBoardManager.Instance.SetScreenActive(0);
            
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
            
            EventsAndFixturesModule.Instance.SetFixturesPanel();
            DashBoardManager.Instance.SetGameObjectActive(false, "ScreenSelector");
            TransferListEntry.Instance.InstantiateTeamSheet("Transfer");
            DashBoardManager.Instance.LoadTransferList();
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

        #region Login
        
        // Login button
        public void OnClickLogin()
        {
            var request = new LoginWithEmailAddressRequest { Email = _userEmail, Password = _userPassword };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
        }
        
        /// <summary>
        /// If login successful;
        /// Set player preferences.
        /// Start loading player data and files.
        /// </summary>
        /// <param name="result"></param>
        private void OnLoginSuccess(LoginResult result)
        {
            Debug.Log("Boom!! Successful API Login." + Environment.NewLine + "Email: " + _userEmail);
            
            PlayerPrefs.SetString("EMAIL", _userEmail);
            PlayerPrefs.SetString("PASSWORD", _userPassword);

            EntityId = result.EntityToken.Entity.Id;
            EntityType = result.EntityToken.Entity.Type;
            
            StartCoroutine(TeamSheetDatabase.Instance.WaitForPlayFabLogin());    // wait for playfab player login and load player data
        }
        
        /// <summary>
        /// If auto login fails
        /// </summary>
        /// <param name="error"></param>
        private void OnLoginFailure(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());

            DisplayLoginRegisterFailure(error);
        }
        # endregion

        #region Register

        public void OnClickRegister()
        {
            var registerRequest = new RegisterPlayFabUserRequest {Email = _userEmail, Password = _userPassword, Username = _userName};
            PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
        }
                
        private void OnRegisterSuccess(RegisterPlayFabUserResult result)
        { 
            Debug.Log("Boom!! Successful API Registration.");
                    
            PlayerPrefs.SetString("EMAIL", _userEmail); 
            PlayerPrefs.SetString("PASSWORD", _userPassword);
                    
            PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest {DisplayName = _userName}, OnDisplayNameUpdated, ErrorCallback);
                    
            OnAddUsernameToAccount();
        }

        private void OnRegisterFailure(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
                    
            DisplayLoginRegisterFailure(error);
        }

        // Login button for AddLoginPanel
        // Adds playfab username/password auth to an existing account created via an anonymous auth method, e.g. automatic device ID login.
        private void OnAddUsernameToAccount()
        {
            var addLoginRequest = new AddUsernamePasswordRequest() {Email = _userEmail, Password = _userPassword, Username = _userName};
            PlayFabClientAPI.AddUsernamePassword(addLoginRequest, OnAddUsernameSuccess, ErrorCallback);
        }
                
        private void OnAddUsernameSuccess(AddUsernamePasswordResult result)
        {
            Debug.Log("Boom!! Successful username update.");
                    
            PlayerPrefs.SetString("EMAIL", _userEmail);
            PlayerPrefs.SetString("PASSWORD", _userPassword);
        }

        #endregion

        #region General

        /// <summary>
        /// Generates PlayFab Error Report
        /// </summary>
        /// <param name="error"></param>
        public void ErrorCallback(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }

        public void OnDisplayNameUpdated(UpdateUserTitleDisplayNameResult result)
        {
            Debug.Log("DisplayName Updated!" + Environment.NewLine + "DisplayName/Username: " + result.DisplayName);
        }

        private void DisplayLoginRegisterFailure(PlayFabError error)
        {
            var errorTextObj = GameObjectFinder.FindSingleObjectByName("LoginErrorText");
            errorTextObj.GetComponent<TMP_Text>().text = error.GenerateErrorReport();
                    
            Instance.StartCoroutine(SetActiveFor(4f, errorTextObj));
        }

        private IEnumerator SetActiveFor(float activeTime, GameObject obj)
        {
            obj.SetActive(true);
            yield return new WaitForSeconds(activeTime);
            obj.SetActive(false);
        }
                
        // returns device ID
        private string ReturnMobileId()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            return deviceId;
        }

        #endregion

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
    }
}