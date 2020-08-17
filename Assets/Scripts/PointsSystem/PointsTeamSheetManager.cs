using System;
using System.Collections.Generic;
using System.Linq;
using Dashboard;
using GoogleSheetsLevelSynchronizer;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    public class PointsTeamSheetManager : MonoBehaviour
    {
        private GameObject _pointsTeamSheet;
        private int _currentGameweekIndex = 0;
        private const int NoOfGameweeks = 3;
        
        [SerializeField] private Dictionary<string, GameObject> playerMap = new Dictionary<string, GameObject>();
        
        private static Dictionary<string,string[]> _gameweekPlayerPointsMap = new Dictionary<string, string[]>();

        public void Start()
        {
            SetPlayerPoints(0);
        }

        private void SetPlayerPoints(int currentGwIndex)
        {
            if (playerMap.Count == 0)
                GetPlayerMap();
            if (_gameweekPlayerPointsMap.Count == 0)
                GetGameweekPoints();

            foreach (var pair1 in playerMap)
            {
                var playerTeamEntryCanvas = pair1.Value;
                var footballPlayerDetails = playerTeamEntryCanvas.GetComponent<FootballPlayerDetails>();

                foreach (var pair2 in _gameweekPlayerPointsMap)
                {
                    if (pair2.Key == footballPlayerDetails.playerName)
                    {
                        footballPlayerDetails.gameweekPoints = pair2.Value[currentGwIndex];
                    }
                }

                var playerTeamEntryCanvasGrandChildren = GetGrandChildren(playerTeamEntryCanvas);
                var priceScoreObj = playerTeamEntryCanvasGrandChildren.Find(x => x.name == "PriceScore");
                priceScoreObj.GetComponent<TMP_Text>().text = footballPlayerDetails.gameweekPoints;
            }
        }

        private void SetGameweekTitleNo(string gameweekNo)
        {
            var title = GameObjectFinder.FindSingleObjectByName("TotalGameweekPointsTitle");
            title.GetComponent<TMP_Text>().text = "Gameweek " + gameweekNo + " Points";
        }

        private void GetPlayerMap()
        {
            _pointsTeamSheet = GameObjectFinder.FindSingleObjectByName("PointsTeamSheet");
            var grandChildrenOfTeamSheet = GetGreatGrandChildren(_pointsTeamSheet);

            for (int i = 0; i < grandChildrenOfTeamSheet.Count; i++)
            {
                var playerObj = grandChildrenOfTeamSheet[i];

                if (playerObj.GetComponent<FootballPlayerDetails>() == null)
                    continue;
                var playerName = playerObj.GetComponent<FootballPlayerDetails>().playerName;
                
                playerMap.Add(playerName, playerObj);
            }
            Debug.LogError(playerMap.Count);
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

        private List<GameObject> GetGreatGrandChildren(GameObject canvas)
        {
            var grandchildren = new List<GameObject>();
            var panel = canvas.transform.GetChild(0).GetChild(0);

            for (int i = 0; i < panel.childCount; i++)
            {
                var playerObj = panel.GetChild(i).gameObject;
                grandchildren.Add(playerObj);
            }

            return grandchildren;
        }
        
        private void GetGameweekPoints()
        {
            var sheet = GoogleSheetReader.Reader("1iufkvofC9UcmJS5ld3R72RJZHz2kFd97BYR-1kL8XeM", "Sheet2!A3:F203");
            
            foreach (var list in sheet)
            {
                var playerNameData = list[1].ToString();
                var namePos = TransferListWindow.AnalyzePlayerNameData(playerNameData);
                var playerName = namePos[0];
                var gameweekPoints = new string[]
                {
                    list[3].ToString(),       // Gw1
                    list[4].ToString(),       // Gw2
                    list[5].ToString()        // Gw3
                };
                
                if (namePos[0] == null)
                    continue;

                if (!_gameweekPlayerPointsMap.ContainsKey(playerName))
                {
                    _gameweekPlayerPointsMap.Add(playerName, gameweekPoints);
                }
                else
                {
                    Debug.LogError("gameweekPlayerPointsMap already contains player");
                }
            }
        }

        public void GameweekPointsButtonRight()
        {
            _currentGameweekIndex = UpIndex(_currentGameweekIndex, 3);
            SetPlayerPoints(_currentGameweekIndex);
            SetGameweekTitleNo(_currentGameweekIndex.ToString());
        }

        public void GameweekPointsButtonLeft()
        {
            _currentGameweekIndex = DownIndex(_currentGameweekIndex);
            SetPlayerPoints(_currentGameweekIndex);
            SetGameweekTitleNo(_currentGameweekIndex.ToString());
        }
        
        private int DownIndex(int index)
        {
            index--;

            if (index >= 0) 
                return index;
            
            index = 0;
            return index;

        }

        private int UpIndex(int index, int arrayCount)
        {
            index++;

            if (index <= arrayCount - 1) 
                return index;
            
            index = arrayCount - 1;
            return index;

        }
    }
}