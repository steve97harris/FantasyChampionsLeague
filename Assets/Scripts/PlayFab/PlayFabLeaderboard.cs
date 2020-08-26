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
            else
            {
                if (Instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
        }

        public void GetLeaderboard()
        {
            PlayFabPlayerStats.PlayFabPlayerStatistics.SetStatistics();
            
            var requestLeaderboard = new GetLeaderboardRequest
            {
                StartPosition = 0, 
                StatisticName = "CoachTotalPoints", 
                MaxResultsCount = 20
            };
            PlayFabClientAPI.GetLeaderboard(requestLeaderboard, OnGetLeaderboard, OnLeaderboardError);
        }

        private void OnGetLeaderboard(GetLeaderboardResult result)
        {
            SetLeaderboardUi(result);
            
            // foreach (var player in result.Leaderboard)
            // {
            //     Debug.Log(player.Position + " " + player.DisplayName + ", " + player.StatValue);
            // }
        }

        private void OnLeaderboardError(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }

        private void SetLeaderboardUi(GetLeaderboardResult result)
        {
            var leaderboardPanelViewport = GameObjectFinder.FindSingleObjectByName("LeaderboardPanelViewport");
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