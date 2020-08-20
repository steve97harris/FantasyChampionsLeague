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

        public static void GetPlayerTransferList(List<string> playerDatabaseCsv)
        {
            // span each line of playerList
            foreach (var line in playerDatabaseCsv)
            {
                var playerInformation = line.Split(',');

                var playerTeam = playerInformation[0];
                var nameData = playerInformation[2];
                var positionData = playerInformation[3];
                var playerRating = playerInformation[4];
                var playerPrice = playerInformation[5];
                var playerFclPoints = playerInformation[6];

                var playerName = AnalyzePlayerNameData(nameData);
                var playerPosition = AnalyzePlayerPositionData(positionData);

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

        public static string AnalyzePlayerNameData(string nameData)
        {
            for (int i = 0; i < nameData.Length; i++)
            {
                if (int.TryParse(nameData[i].ToString(), out var res))
                {
                    nameData = nameData.Remove(i, nameData.Length - i);
                }
            }
            var playerName = nameData;

            return playerName;
        }

        public static string AnalyzePlayerPositionData(string positionData)
        {
            // needs adjusting
            var playerPosition = "";
            if (positionData.Contains("FW") && positionData.Contains("AM") || positionData.Contains("FW"))
                playerPosition = "F";
            else if (positionData.Contains("D"))
                playerPosition = "D";
            else if (positionData.Contains("GK"))
                playerPosition = "Gk";
            else if (positionData.Contains("M") || positionData.Contains("AM"))
                playerPosition = "M";

            return playerPosition;
        }

        public static void InitializePlayerList(Dictionary<string, string[]> playerPricesMap)
        {
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
                    TotalPoints = pair.Value[4]
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