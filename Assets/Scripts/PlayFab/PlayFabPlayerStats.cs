using System;
using System.Collections.Generic;
using DefaultNamespace;
using PlayFab.ClientModels;
using UnityEngine;

namespace PlayFab
{
    public class PlayFabPlayerStats : MonoBehaviour
    {
        public static PlayFabPlayerStats Instance;
        public int coachTotalPoints;
        public int coachCurrentGwPoints;

        #region PlayerStats

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        /// <summary>
        /// Updates the values of the specified title-specific statistics for the user. By default, clients are not permitted to update statistics.
        /// Developers may override this setting in the Game Manager > Settings > API Features.
        /// </summary>
        public void SetStatistics()
        {
            var headCoachSaveData = PlayFabEntityFileManager.Instance.GetHeadCoachData();
            coachTotalPoints = headCoachSaveData.CoachTotalPoints;
            coachCurrentGwPoints = headCoachSaveData.CoachCurrentGwPoints;
            
            PlayFabClientAPI.UpdatePlayerStatistics( new UpdatePlayerStatisticsRequest {
                    // request.Statistics is a list, so multiple StatisticUpdate objects can be defined if required.
                    Statistics = new List<StatisticUpdate> {
                        new StatisticUpdate { StatisticName = "CoachTotalPoints", Value = coachTotalPoints },
                        new StatisticUpdate { StatisticName = "CurrentGwPoints", Value = coachCurrentGwPoints }
                    }
                },
                result =>
                {
                    Debug.Log("User statistics updated");
                    GetStatistics();
                },
                error => { Debug.LogError(error.GenerateErrorReport()); });
        }

        /// <summary>
        /// Retrieves the indicated statistics (current version and values for all statistics, if none are specified), for the local player.
        /// </summary>
        private void GetStatistics()
        {
            PlayFabClientAPI.GetPlayerStatistics(new GetPlayerStatisticsRequest(), OnGetStatistics, error => Debug.LogError(error.GenerateErrorReport()));
        }

        private void OnGetStatistics(GetPlayerStatisticsResult result)
        {
            Debug.Log("Received the following statistics:");
            foreach (var statisticValue in result.Statistics)
            {
                Debug.Log(statisticValue.StatisticName + ": " + statisticValue.Value);

                switch (statisticValue.StatisticName)
                {
                    case "CoachTotalPoints":
                        coachTotalPoints = statisticValue.Value;
                        break;
                    case "CurrentGwPoints":
                        coachCurrentGwPoints = statisticValue.Value;
                        break;
                }
            }
        }

        #endregion
    }
}