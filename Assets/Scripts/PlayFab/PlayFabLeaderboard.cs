using System;
using Dashboard;
using DefaultNamespace;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlayFab
{
    public class PlayFabLeaderboard : MonoBehaviour
    {
        public static PlayFabLeaderboard Instance;

        private void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        /// <summary>
        /// Gets players most up to date statistics.
        /// Retrieves a list of all ranked users for the given statistic, starting from the indicated point in the leaderboard
        /// </summary>
        public void GetLeaderboard()
        {
            PlayFabPlayerStats.Instance.SetStatistics();
            
            var requestLeaderboard = new GetLeaderboardRequest
            {
                StartPosition = 0, 
                StatisticName = "CoachTotalPoints", 
                MaxResultsCount = 20
            };
            PlayFabClientAPI.GetLeaderboard(requestLeaderboard, OnGetLeaderboard, PlayFabController.Instance.ErrorCallback);
        }

        private void OnGetLeaderboard(GetLeaderboardResult result)
        {
            SetLeaderboardUi(result, "LeaderboardPanelViewport");
        }

        /// <summary>
        /// Instantiates the specified leaderboard
        /// </summary>
        /// <param name="result"></param>
        /// <param name="leaderboardViewportName"></param>
        public void SetLeaderboardUi(GetLeaderboardResult result, string leaderboardViewportName)
        {
            var leaderboardPanelViewport = GameObjectFinder.FindSingleObjectByName(leaderboardViewportName);
            var leaderboardEntry = Resources.Load<GameObject>("Prefabs/Leaderboards/LeaderboardEntry");
            
            foreach (Transform child in leaderboardPanelViewport.transform)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var player in result.Leaderboard)
            {
                var playerLeaderboardEntry = Instantiate(leaderboardEntry, leaderboardPanelViewport.transform);
                playerLeaderboardEntry.transform.GetChild(0).GetComponent<TMP_Text>().text = player.Position.ToString();
                playerLeaderboardEntry.transform.GetChild(1).GetComponent<TMP_Text>().text = player.DisplayName;
                playerLeaderboardEntry.transform.GetChild(2).GetComponent<TMP_Text>().text =
                    player.StatValue.ToString();
            }
        }
    }
}