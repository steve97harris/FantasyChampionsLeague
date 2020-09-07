using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace PlayFab
{
    public class PlayFabFriends : MonoBehaviour
    {
        public static PlayFabFriends Instance;
        private List<FriendInfo> _friends;
        private List<FriendInfo> _myFriends;

        private string _friendSearch;

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        /// <summary>
        /// Retrieves a list of ranked friends of the current player for the given statistic, starting from the indicated point in the leaderboard
        /// </summary>
        public void GetFriendLeaderboard()
        {
            PlayFabClientAPI.GetFriendLeaderboard(new GetFriendLeaderboardRequest
            {
                IncludeSteamFriends = false,
                IncludeFacebookFriends = false,
                StartPosition = 0,
                StatisticName = "CoachTotalPoints",
                MaxResultsCount = 100
            }, OnGetLeaderboard, PlayFabController.Instance.ErrorCallback);
        }
        
        private void OnGetLeaderboard(GetLeaderboardResult result)
        {
            PlayFabLeaderboard.Instance.SetLeaderboardUi(result, "FriendsLeaderboardPanelViewport");
        }

        /// <summary>
        /// Where is this used?
        /// </summary>
        public void RunWaitFunction()
        {
            StartCoroutine(WaitForFriend());
        }

        private IEnumerator WaitForFriend()
        {
            yield return new WaitForSeconds(2);
            GetFriendLeaderboard();
        }
        
        private enum FriendIdType
        {
            PlayFabId,
            Username,
            Email,
            DisplayName
        }

        
        /// <summary>
        /// Adds the PlayFab user, based upon a match against a supplied unique identifier, to the friend list of the local user.
        /// At least one of FriendPlayFabId,FriendUsername,FriendEmail, or FriendTitleDisplayName should be initialized.
        /// </summary>
        /// <param name="idType"></param>
        /// <param name="friendId"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void AddFriend(FriendIdType idType, string friendId)
        {
            var request = new AddFriendRequest();
            switch (idType)
            {
                case FriendIdType.PlayFabId:
                    request.FriendPlayFabId = friendId;
                    break;
                case FriendIdType.Username:
                    request.FriendUsername = friendId;
                    break;
                case FriendIdType.Email:
                    request.FriendEmail = friendId;
                    break;
                case FriendIdType.DisplayName:
                    request.FriendTitleDisplayName = friendId;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(idType), idType, null);
            }
            PlayFabClientAPI.AddFriend(request, result =>
            {
                Debug.Log("Friend added successfully");
            }, PlayFabController.Instance.ErrorCallback);
        }

        public void InputFriendId()
        {
            var addFriendInputField = GameObjectFinder.FindSingleObjectByName("AddFriendInputField");
            _friendSearch = addFriendInputField.GetComponent<TMP_InputField>().text;
        }

        public void SubmitFriendRequest()
        {
            AddFriend(FriendIdType.PlayFabId, _friendSearch);
        }
        
        // private void DisplayFriends(List<FriendInfo> friendsCache)
        // {
        //     var leaderboardEntry = Resources.Load<GameObject>("Prefabs/Leaderboards/LeaderboardEntry");
        //     var friendsLeaderboardPanelViewport =
        //         GameObjectFinder.FindSingleObjectByName("FriendsLeaderboardPanelViewport");
        //
        //     foreach (var friendInfo01 in friendsCache)
        //     {
        //         var friendFound = false;
        //
        //         if (_myFriends != null)
        //         {
        //             foreach (var friendInfo02 in _myFriends)
        //             {
        //                 if (friendInfo01.FriendPlayFabId == friendInfo02.FriendPlayFabId)
        //                     friendFound = true;
        //             }
        //         }
        //         
        //         if (friendFound) 
        //             continue;
        //         
        //         var listing = Instantiate(leaderboardEntry, friendsLeaderboardPanelViewport.transform);
        //         listing.transform.GetChild(1).GetComponent<TMP_Text>().text = friendInfo01.TitleDisplayName;
        //         
        //         if (friendInfo01.Profile.Statistics == null) 
        //             continue;
        //         
        //         var friendTotalPoints = friendInfo01.Profile.Statistics[0].Value;
        //         listing.transform.GetChild(2).GetComponent<TMP_Text>().text = friendTotalPoints.ToString();
        //     }
        //
        //     _myFriends = friendsCache;
        // }
    }
}