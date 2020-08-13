using System;
using System.Collections.Generic;
using Dashboard;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    public class PointsTeamSheetManager : MonoBehaviour
    {
        private GameObject _pointsTeamSheet;
        
        [SerializeField] private Dictionary<string, GameObject> playerMap = new Dictionary<string, GameObject>();

        public void Start()
        {
            GetPlayerMap();
            SetPlayerPoints();
        }

        private void SetPlayerPoints()
        {
            foreach (var pair in playerMap)
            {
                var playerTeamEntryCanvas = pair.Value;
                var footballPlayerDetails = playerTeamEntryCanvas.GetComponent<FootballPlayerDetails>();
                
                var playerTeamEntryCanvasGrandChildren = GetGrandChildren(playerTeamEntryCanvas);
                var priceScoreObj = playerTeamEntryCanvasGrandChildren.Find(x => x.name == "PriceScore");
                priceScoreObj.GetComponent<TMP_Text>().text = footballPlayerDetails.points; // - playerScore 
            }
        }

        private void GetPlayerMap()
        {
            _pointsTeamSheet = GameObjectFinder.FindSingleObjectByName("PointsTeamSheet");
            var grandChildrenOfTeamSheet = GetGrandChildren(_pointsTeamSheet);

            for (int i = 0; i < grandChildrenOfTeamSheet.Count; i++)
            {
                var playerObj = grandChildrenOfTeamSheet[i];
                var playerName = playerObj.GetComponent<FootballPlayerDetails>().playerName;

                if (playerName == null)
                    continue;
                
                playerMap.Add(playerName, playerObj);
            }
        }

        private List<GameObject> GetGrandChildren(GameObject canvas)
        {
            var grandchildren = new List<GameObject>();
            var panel = canvas.transform.GetChild(0);

            for (int i = 0; i < panel.childCount; i++)
            {
                var playerObj = panel.GetChild(i).gameObject;
                grandchildren.Add(playerObj);
            }

            return grandchildren;
        }
    }
}