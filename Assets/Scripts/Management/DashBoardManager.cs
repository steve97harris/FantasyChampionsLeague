using System.Collections;
using System.Collections.Generic;
using CSV;
using Dashboard;
using DefaultNamespace;
using GoogleSheetsLevelSynchronizer;
using UnityEngine;
using Newtonsoft.Json;

public class DashBoardManager : MonoBehaviour
{
    private readonly List<string> _fclScreenNames = new List<string>()
    {
        "LoginPanel",
        "DashBoard",
        "PointsPage",
        "TransfersPage",
        "FixturesResults"
    };
    
    void Start()
    {
        SetScreenActive(1);
        SetScreenSelectorActive();

        InitiateTransferList();
    }

    public void PointsButton()
    {
        SetScreenActive(2); 
    }

    public void TransfersButton()
    {
        SetScreenActive(3);
    }

    public void DashboardButton()
    {
        SetScreenActive(1);
    }

    public void FixturesResultsButton()
    {
        SetScreenActive(4);
    }

    private void InitiateTransferList()
    {
        // var sheet = GoogleSheetReader.Reader("1iufkvofC9UcmJS5ld3R72RJZHz2kFd97BYR-1kL8XeM", "A3:E173");

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

    private void SetScreenSelectorActive()
    {
        var screenSelector = GameObjectFinder.FindSingleObjectByName("ScreenSelector");
        screenSelector.SetActive(true);
    }
}
