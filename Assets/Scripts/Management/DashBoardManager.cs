using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Dashboard;
using GoogleSheetsLevelSynchronizer;
using UnityEngine;
using Newtonsoft.Json;
using PlayFab;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    public class DashBoardManager : MonoBehaviour
    {
        public static DashBoardManager Instance;

        #region File Paths

        public const string FileNameA = "FootballPlayerDatabase.csv";
        public const string FileNameB = "FootballPlayerPointsDatabase.csv";

        public string footballPlayerDatabasePath = Path.Combine(Application.streamingAssetsPath,FileNameA);
        public string defaultFootballPlayerPointsDatabasePath = Path.Combine(Application.streamingAssetsPath, FileNameB);

        #endregion
        
        /// <summary>
        /// List of all main panels
        /// </summary>
        private readonly List<string> _mainPanelNames = new List<string>()
        {
            "LoginPanel", "DashBoardPanel", "PointsPanel", "TransferPanel", "LeagueLeaderboardsPanel", "PlayerProfilePanel", "LoadingPanel", "FixturesPanel"
        };

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        #region Dashboard Buttons

        public void PointsButton()
        {
            SetScreenActive(2);
        }

        public void TransfersButton()
        {
            SetScreenActive(3);
            SetTransferTeamSheet();
        }

        public void DashboardButton()
        {
            SetScreenActive(1);
        }

        public void WorldPlayerLeagueButton()
        {
            SetScreenActive(4);
            PlayFabLeaderboard.Instance.GetLeaderboard();
            SetLeaderboardPanel("world");
        }
        
        public void FriendPlayerLeagueButton()
        {
            SetScreenActive(4);
            PlayFabFriends.Instance.GetFriendLeaderboard();
            SetLeaderboardPanel("friend");
        }

        public void PlayerProfileButton()
        {
            SetScreenActive(5);
            PlayFabAccountInformation.Instance.GetAccountInfo();
        }

        public void FixturesPanel()
        {
            SetScreenActive(7);
        }

        #endregion

        /// <summary>
        /// Gets list of footballers in CSV, then analysis the footballers information
        /// </summary>
        public void LoadTransferList()
        {
            var footballPlayerDatabase = CsvReader.LoadCsvFileViaPath(footballPlayerDatabasePath);
            TransferListWindow.Instance.GetPlayerTransferList(footballPlayerDatabase);
        }
        
        public void ActivateDashBoard()
        {
            SetScreenActive(1);
            SetGameObjectActive(true, "ScreenSelector");
        }

        /// <summary>
        /// Sets screen active with respect to the index entered and sets all other screens inactive.
        /// LoginPanel - 0
        /// DashBoardPanel - 1
        /// PointsPanel - 2
        /// TransferPanel - 3
        /// LeagueLeaderboardsPanel - 4
        /// PlayerProfilePanel - 5
        /// LoadingPanel - 6
        /// FixturesPanel - 7
        /// </summary>
        /// <param name="index"></param>
        public void SetScreenActive(int index)
        {
            for (int i = 0; i < _mainPanelNames.Count; i++)
            {
                GameObjectFinder.FindSingleObjectByName(_mainPanelNames[i]).SetActive(i == index);
            }
        }

        /// <summary>
        /// Sets specified game object active or inactive
        /// </summary>
        /// <param name="active"></param>
        /// <param name="objectName"></param>
        public void SetGameObjectActive(bool active, string objectName)
        {
            var obj = GameObjectFinder.FindSingleObjectByName(objectName);
            
            if (obj == null)
            {
                Debug.LogError(objectName + " returned null");
                return;
            }
            obj.SetActive(active);
        }

        /// <summary>
        /// Sets either friends or world leaderboard panel active
        /// </summary>
        /// <param name="leaderboardName"></param>
        private void SetLeaderboardPanel(string leaderboardName)
        {
            switch (leaderboardName)
            {
                case "world":
                    SetGameObjectActive(true, "WorldLeaderboardPanel");
                    SetGameObjectActive(false, "FriendsPanel");
                    break;
                case "friend":
                    SetGameObjectActive(false, "WorldLeaderboardPanel");
                    SetGameObjectActive(true, "FriendsPanel");
                    break;
            }
        }

        private void SetTransferTeamSheet()
        {
            if (GameObjectFinder.FindSingleObjectByName("TransferTeamSheet(Clone)") != null) 
                return;
            
            TransferListEntry.Instance.InstantiateTeamSheet("Transfer");
        }
    }
}

