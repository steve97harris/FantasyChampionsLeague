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
    private readonly List<string> _fclScreenNames = new List<string>()
    {
        "LoginPanel",
        "DashBoard",
        "PointsPage",
        "TransfersPage",
        "LeagueLeaderboards"
    };
    
    void Start()
    {
        SetScreenActive(0);

        InitiateTransferList();
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
        
        var transferListWindow = GameObjectFinder.FindSingleObjectByName("TransferListWindow");
        var transferListWindowComponent = transferListWindow.GetComponent<TransferListWindow>();
        transferListWindowComponent.TransferListEntryInstantiateTransferTeamSheet();
    }

    public void DashboardButton()
    {
        SetScreenActive(1);
    }

    public void LeaguesButton()
    {
        SetScreenActive(4);
        PlayFabLeaderboard.Instance.GetLeaderboard();
    }

    private void InitiateTransferList()
    {
        var footballPlayerDatabase = CsvReader.LoadCsvFile(Application.streamingAssetsPath + "/FootballPlayerDatabase.csv");
        TransferListWindow.GetPlayerTransferList(footballPlayerDatabase);
    }

    private void SetScreenActive(int index)
    {
        for (int i = 0; i < _fclScreenNames.Count; i++)
        {
            GameObjectFinder.FindSingleObjectByName(_fclScreenNames[i]).SetActive(i == index);
        }
    }

    public static void SetScreenSelectorActive()
    {
        var screenSelector = GameObjectFinder.FindSingleObjectByName("ScreenSelector");
        screenSelector.SetActive(true);
    }
}
