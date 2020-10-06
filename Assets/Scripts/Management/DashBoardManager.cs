using System;
using System.Collections;
using System.Collections.Generic;
using CSV;
using Dashboard;
using DefaultNamespace;
using GoogleSheetsLevelSynchronizer;
using UnityEngine;
using Newtonsoft.Json;
using PlayFab;
using UnityEngine.Serialization;

public class DashBoardManager : MonoBehaviour
{
    public static DashBoardManager Instance;

    #region File Paths

    public string footballPlayerDatabasePath = Application.streamingAssetsPath + "/FootballPlayerDatabase.csv";
    public string footballPlayerPointsDatabasePath = Application.streamingAssetsPath + "/FootballPlayerPointsDatabase.csv";

    #endregion
    
    /*
     *  ObjectName - Index 
     * "LoginPanel" - 0
     * "DashBoardPanel" - 1
     * "PointsPanel" - 2
     * "TransferPanel" - 3
     * "LeagueLeaderboardsPanel" - 4
     * "PlayerProfilePanel" - 5
     * "AddLoginPanel" - 6
     * "LoadingPanel" - 7
     * "FixturesPanel" - 8
     */
    /// <summary>
    /// List of all main panels
    /// </summary>
    private readonly List<string> _mainPanelNames = new List<string>()
    {
        "LoginPanel", "DashBoardPanel", "PointsPanel", "TransferPanel", "LeagueLeaderboardsPanel", "PlayerProfilePanel", "AddLoginPanel", "LoadingPanel", "FixturesPanel"
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
        SetScreenActive(8);
    }

    #endregion

    /// <summary>
    /// Gets list of footballers in CSV, then analysis the footballers information
    /// </summary>
    public void LoadTransferList()
    {
        var footballPlayerDatabase = CsvReader.LoadCsvFile(this.footballPlayerDatabasePath);
        TransferListWindow.Instance.GetPlayerTransferList(footballPlayerDatabase);
    }

    /// <summary>
    /// Sets screen active with respect to the index entered and sets all other screens inactive
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
