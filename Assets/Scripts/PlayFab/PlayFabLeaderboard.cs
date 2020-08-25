using PlayFab.ClientModels;
using UnityEngine;

namespace PlayFab
{
    public class PlayFabLeaderboard : MonoBehaviour
    {
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
            foreach (var player in result.Leaderboard)
            {
                Debug.Log(player.Position + " " + player.DisplayName + ", " + player.StatValue);
            }
        }

        private void OnLeaderboardError(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }
    }
}