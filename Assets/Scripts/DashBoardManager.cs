using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class DashBoardManager : MonoBehaviour
{
    private GameObject dashboard = null;
    private GameObject pointsPage = null;
    private GameObject teamPage = null;
    private GameObject transfersPage = null;

    void Start()
    {
        dashboard = GameObjectFinder.FindSingleObjectByName("Dashboard");
        pointsPage = GameObjectFinder.FindSingleObjectByName("PointsPage");
        teamPage = GameObjectFinder.FindSingleObjectByName("TeamPage");
        
        dashboard.SetActive(true);
        pointsPage.SetActive(false);
        teamPage.SetActive(false);
        transfersPage.SetActive(false);
    }

    public void PointsButton()
    {
        pointsPage.SetActive(true);
        dashboard.SetActive(false);
        teamPage.SetActive(false);
        transfersPage.SetActive(false);
    }

    public void TeamButton()
    {
        teamPage.SetActive(true);
        dashboard.SetActive(false);
        pointsPage.SetActive(false);
        transfersPage.SetActive(false);
    }

    public void TransfersButton()
    {
        transfersPage.SetActive(true);
        dashboard.SetActive(false);
        pointsPage.SetActive(false);
        teamPage.SetActive(false);
    }
}
