using System.Collections;
using System.Collections.Generic;
using Dashboard;
using DefaultNamespace;
using GoogleSheetsLevelSynchronizer;
using UnityEngine;
using Newtonsoft.Json;

public class DashBoardManager : MonoBehaviour
{
    private readonly List<string> _fclScreenNames = new List<string>()
    {
        "DashBoard",
        "PointsPage",
        "TeamPage",
        "TransfersPage"
    };
    
    void Start()
    {
        SetScreenActive(0);

        InitiateTransferList();
    }

    public void PointsButton()
    {
        SetScreenActive(1);
    }

    public void TeamButton()
    {
        SetScreenActive(2);
    }

    public void TransfersButton()
    {
        SetScreenActive(3);
    }

    private void InitiateTransferList()
    {
        var sheet = GoogleSheetReader.Reader("1iufkvofC9UcmJS5ld3R72RJZHz2kFd97BYR-1kL8XeM", "A3:C173");
        TransferListWindow.GetPlayerTransferList(sheet);
    }

    private void SetScreenActive(int index)
    {
        for (int i = 0; i < _fclScreenNames.Count; i++)
        {
            GameObjectFinder.FindSingleObjectByName(_fclScreenNames[i]).SetActive(i == index);
        }
    }
}
