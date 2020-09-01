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

    void Start()
    {
        SetScreenActive(6);
        SetScreenSelectorActive(false);
        LoadTransferList();
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
        
        InstantiateTeamSheet("Transfer");
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

    private void LoadTransferList()
    {
        var footballPlayerDatabase = CsvReader.LoadCsvFile(Application.streamingAssetsPath + "/FootballPlayerDatabase.csv");
        TransferListWindow.GetPlayerTransferList(footballPlayerDatabase);
    }

    private void InstantiateTeamSheet(string teamSheetName)
    {
        var transferListWindow = GameObjectFinder.FindSingleObjectByName("TransferListWindow");
        var transferListWindowComponent = transferListWindow.GetComponent<TransferListWindow>();
        transferListWindowComponent.TransferListEntryInstantiateTransferTeamSheet(teamSheetName);
    }

    public void SetScreenActive(int index)
    {
        for (int i = 0; i < _fclScreenNames.Count; i++)
        {
            GameObjectFinder.FindSingleObjectByName(_fclScreenNames[i]).SetActive(i == index);
        }
    }

    public void SetScreenSelectorActive(bool active)
    {
        var screenSelector = GameObjectFinder.FindSingleObjectByName("ScreenSelector");
        screenSelector.SetActive(active);
    }

    private void SetLeaderboardPanel(string leaderboardName)
    {
        var worldLeaderboard = GameObjectFinder.FindSingleObjectByName("WorldLeaderboardPanel");
        var friendLeaderboard = GameObjectFinder.FindSingleObjectByName("FriendsPanel");

        switch (leaderboardName)
        {
            case "world":
                worldLeaderboard.SetActive(true);
                friendLeaderboard.SetActive(false);
                break;
            case "friend":
                worldLeaderboard.SetActive(false);
                friendLeaderboard.SetActive(true);
                break;
        }
    }
}
