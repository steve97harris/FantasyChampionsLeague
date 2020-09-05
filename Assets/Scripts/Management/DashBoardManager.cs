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

public class DashBoardManager : MonoBehaviour
{
    public static DashBoardManager Instance;
    
    /*  ObjectName - Index 
     * "LoginPanel" - 0
     * "DashBoardPanel" - 1
     * "PointsPanel" - 2
     * "TransferPanel" - 3
     * "LeagueLeaderboardsPanel" - 4
     * "PlayerProfilePanel" - 5
     * "LoadingPanel" - 6
     * "AddLoginPanel" - 7
     */
    private readonly List<string> _fclScreenNames = new List<string>()
    {
        "LoginPanel",
        "DashBoardPanel",
        "PointsPanel",
        "TransferPanel",
        "LeagueLeaderboardsPanel",
        "PlayerProfilePanel",
        "LoadingPanel",
        "AddLoginPanel"
    };

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public void PointsButton()
    {
        SetScreenActive(2);
    }

    public void TransfersButton()
    {
        SetScreenActive(3);

        if (GameObjectFinder.FindSingleObjectByName("TransferTeamSheet(Clone)") != null) 
            return;
        
        TransferListEntry.Instance.InstantiateTeamSheet("Transfer");
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

    public void LoadTransferList()
    {
        var footballPlayerDatabase = CsvReader.LoadCsvFile(Application.streamingAssetsPath + "/FootballPlayerDatabase.csv");
        TransferListWindow.GetPlayerTransferList(footballPlayerDatabase);
    }

    public void SetScreenActive(int index)
    {
        for (int i = 0; i < _fclScreenNames.Count; i++)
        {
            GameObjectFinder.FindSingleObjectByName(_fclScreenNames[i]).SetActive(i == index);
        }
    }

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
}
