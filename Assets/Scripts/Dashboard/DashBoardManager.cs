using System.Collections;
using System.Collections.Generic;
using Dashboard;
using DefaultNamespace;
using GoogleSheetsLevelSynchronizer;
using UnityEngine;
using Newtonsoft.Json;

public class DashBoardManager : MonoBehaviour
{
    private static GameObject _dashboard = null;
    private static GameObject _pointsPage = null;
    private static GameObject _teamPage = null;
    private static GameObject _transfersPage = null;

    void Start()
    {
        _dashboard = GameObjectFinder.FindSingleObjectByName("DashBoard");
        _pointsPage = GameObjectFinder.FindSingleObjectByName("PointsPage");
        _teamPage = GameObjectFinder.FindSingleObjectByName("TeamPage");
        _transfersPage = GameObjectFinder.FindSingleObjectByName("TransfersPage");

        _dashboard.SetActive(true);
        _pointsPage.SetActive(false);
        _teamPage.SetActive(false);
        _transfersPage.SetActive(false);

        LoadPlayerTransferListWindow();
    }

    public void PointsButton()
    {
        _pointsPage.SetActive(true);
        _dashboard.SetActive(false);
        _teamPage.SetActive(false);
        _transfersPage.SetActive(false);
    }

    public void TeamButton()
    {
        _teamPage.SetActive(true);
        _dashboard.SetActive(false);
        _pointsPage.SetActive(false);
        _transfersPage.SetActive(false);
    }

    public void TransfersButton()
    {
        Debug.LogError(_transfersPage.name);
        
        _transfersPage.SetActive(true);
        _dashboard.SetActive(false);
        _pointsPage.SetActive(false);
        _teamPage.SetActive(false);
    }

    private void LoadHome()
    {
        
    }

    private void LoadPlayerTransferListWindow()
    {
        var sheet = GoogleSheetReader.Reader("1iufkvofC9UcmJS5ld3R72RJZHz2kFd97BYR-1kL8XeM", "A3:C173");
        PlayerTransferListWindow.GetPlayerTransferList(sheet);
    }
}
