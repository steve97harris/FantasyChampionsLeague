using DefaultNamespace;
using PlayFab.ClientModels;
using UnityEngine;

namespace PlayFab
{
    public class PlayFabLogin : MonoBehaviour
    {
        private string _userEmail;
        private string _userPassword;
        private string _userName;
        
        public void Start()
        {
            if (string.IsNullOrEmpty(PlayFabSettings.TitleId))
            {
                PlayFabSettings.TitleId = "90D7E";
            }

            if (PlayerPrefs.HasKey("EMAIL"))
            {
                _userEmail = PlayerPrefs.GetString("EMAIL");
                _userPassword = PlayerPrefs.GetString("PASSWORD");
#if UNITY_STANDALONE || UNITY_EDITOR
                var request = new LoginWithEmailAddressRequest { Email = _userEmail, Password = _userPassword };
                PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
#endif
            }
            else
            {
#if UNITY_IOS
                
#endif
            }
        }

        private void OnLoginSuccess(LoginResult result)
        {
            Debug.Log("Boom!! Successful API Login.");
            
            PlayerPrefs.SetString("EMAIL", _userEmail);
            PlayerPrefs.SetString("PASSWORD", _userPassword);
            
            ActivateDashBoard();
        }

        private void OnRegisterSuccess(RegisterPlayFabUserResult result)
        {
            Debug.Log("Boom!! Successful API Registration.");
            
            PlayerPrefs.SetString("EMAIL", _userEmail);
            PlayerPrefs.SetString("PASSWORD", _userPassword);
            
            ActivateDashBoard();
        }

        private void OnLoginFailure(PlayFabError error)
        {
            var registerRequest = new RegisterPlayFabUserRequest {Email = _userEmail, Password = _userPassword, Username = _userName};
            PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
        }

        private void OnRegisterFailure(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }

        private void ActivateDashBoard()
        {
            var loginPanel = GameObjectFinder.FindSingleObjectByName("LoginPanel");
            var dashboard = GameObjectFinder.FindSingleObjectByName("DashBoard");
            var screenSelector = GameObjectFinder.FindSingleObjectByName("ScreenSelector");
            loginPanel.SetActive(false);
            dashboard.SetActive(true);
            screenSelector.SetActive(true);
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
    }
}