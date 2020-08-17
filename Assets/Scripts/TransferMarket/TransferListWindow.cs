using System;
using System.Collections.Generic;
using System.Globalization;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dashboard
{
    public class TransferListWindow : MonoBehaviour
    {
        public static readonly Dictionary<string, string[]> PlayerPricesMap = new Dictionary<string, string[]>();        // name, price

        public static void GetPlayerTransferList(IList<IList<object>> googleSheetPlayersList)
        {
            // span each line of playerList
            foreach (var list in googleSheetPlayersList)
            {
                if (list[0].ToString() == "Team")
                    continue;
                
                var playerTeam = list[0].ToString();
                var playerRating = list[2].ToString();
                var playerPrice = list[3].ToString();
                var playerFclPoints = list[4].ToString();

                var nameData = list[1].ToString();
                var playerNameData = AnalyzePlayerNameData(nameData);
                var playerName = playerNameData[0];
                var playerPosition = playerNameData[1];

                var playerDetails = new string[5];
                playerDetails[0] = playerTeam;
                playerDetails[1] = playerRating;
                playerDetails[2] = playerPosition;
                playerDetails[3] = playerPrice;
                playerDetails[4] = playerFclPoints;
                
                if (!PlayerPricesMap.ContainsKey(playerName))
                    PlayerPricesMap.Add(playerName, playerDetails);
                else
                    Debug.LogError("playerPricesMap already contains key: " + playerName);
            }
        }

        private static string[] AnalyzePlayerNameData(string nameData)
        {
            var nameDataSplit = nameData.Split(new [] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
                
            if (nameDataSplit.Length != 2)
                return new string[2];
                
            for (int i = 0; i < nameDataSplit[1].Length; i++)
            {
                if (int.TryParse(nameDataSplit[1][i].ToString(), out var res))
                {
                    nameDataSplit[1] = nameDataSplit[1].Remove(i, nameDataSplit[1].Length - i);
                }
            }
            var playerName = nameDataSplit[1];
            
            var indexOfComa = nameData.IndexOf(",", StringComparison.Ordinal);
            var nameDataPlayerPositions = nameData.Remove(0, indexOfComa);
            // needs adjusting
            var playerPosition = "";
            if (nameDataPlayerPositions.Contains("FW") && nameDataPlayerPositions.Contains("AM") || nameDataPlayerPositions.Contains("FW"))
                playerPosition = "F";
            else if (nameDataPlayerPositions.Contains("D"))
                playerPosition = "D";
            else if (nameDataPlayerPositions.Contains("GK"))
                playerPosition = "Gk";
            else if (nameDataPlayerPositions.Contains("M") || nameDataPlayerPositions.Contains("AM"))
                playerPosition = "M";

            var namePositionArray = new string[] {playerName, playerPosition};

            return namePositionArray;
        }

        public static void InitializePlayerList(Dictionary<string, string[]> playerPricesMap)
        {
            Debug.LogError("hello");
            // locate gameObjects
            var transferListContent = GameObjectFinder.FindSingleObjectByName("TransferListContent").transform;
            var playerTransferEntry = GameObjectFinder.FindSingleObjectByName("PlayerTransferEntry");
            
            var teamLogosObject = GameObjectFinder.FindSingleObjectByName("TeamLogos");
            var teamLogos = teamLogosObject.GetComponent<TeamLogos>().teamLogos;

            foreach (Transform child in transferListContent)
            {
                Destroy(child.gameObject);
            }

            foreach (var pair in playerPricesMap)        
            {
                // instantiate new player transfer entry
                var entryObject = Instantiate(playerTransferEntry, transferListContent);
                var entryButton = entryObject.transform.GetChild(0).transform;
                var playerNameObj = entryButton.GetChild(0).gameObject;
                var playerPriceObj = entryButton.GetChild(1).gameObject;
                var playerTeamImageObj = entryButton.GetChild(2).gameObject;
                var playerPositionObj = entryButton.GetChild(3).gameObject;
                
                // set football players details component
                entryObject.AddComponent<FootballPlayerDetails>();
                TeamSheetDatabase.SetFootballPlayerDetailsValues(entryObject, new AthleteStats()
                {
                    PlayerName = pair.Key,
                    Team = pair.Value[0],
                    Rating = pair.Value[1], 
                    Position = pair.Value[2],
                    Price = pair.Value[3],
                    Points = pair.Value[4]
                });
                
                playerNameObj.GetComponent<TMP_Text>().text = pair.Key;
                playerPriceObj.GetComponent<TMP_Text>().text = "$" + pair.Value[1];
                playerPositionObj.GetComponent<TMP_Text>().text = pair.Value[2];

                // team logos!!!
                var playersTeamLogo = teamLogos.Find(x => x.name == pair.Value[0]);
                if (playersTeamLogo != null)
                    playerTeamImageObj.GetComponent<Image>().sprite = playersTeamLogo;

                entryObject.GetComponent<Canvas>().enabled = true;
                entryObject.GetComponent<CanvasScaler>().enabled = true;
                entryObject.GetComponent<GraphicRaycaster>().enabled = true;
                entryButton.GetComponent<Image>().enabled = true;
                entryButton.GetComponent<Button>().enabled = true;
                entryObject.SetActive(true);
            }
            
        }

        public void BackButton_ToTransferTeamSheet()
        {
            DestroyTransferList_LoadTransferTeamSheet();
        }

        public static void DestroyTransferList_LoadTransferTeamSheet()
        {
            var list = GameObjectFinder.FindSingleObjectByName("TransferList(Clone)");
            var transferTeamSheet = GameObjectFinder.FindSingleObjectByName("TransferTeamSheet");
            
            list.SetActive(false);
            transferTeamSheet.SetActive(true);
            DestroyImmediate(list,true);
        }
    }
}