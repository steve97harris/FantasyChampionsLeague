using System;
using System.Collections.Generic;
using System.Globalization;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dashboard
{
    public class PlayerTransferListWindow : MonoBehaviour
    {
        public static readonly Dictionary<string, string[]> PlayerPricesMap = new Dictionary<string, string[]>();        // name, price

        public static void GetPlayerTransferList(IList<IList<object>> googleSheetPlayersList)
        {
            foreach (var list in googleSheetPlayersList)
            {
                // list[0] - Team
                // list[1] - Name
                // list[2] - Rating
            
                var nameData = list[1].ToString();
                var nameDataSplit = nameData.Split(new [] {"\r\n", "\r", "\n"}, StringSplitOptions.None);
                
                if (nameDataSplit.Length != 2)
                    continue;
                
                for (int i = 0; i < nameDataSplit[1].Length; i++)
                {
                    if (int.TryParse(nameDataSplit[1][i].ToString(), out var res))
                    {
                        nameDataSplit[1] = nameDataSplit[1].Remove(i, nameDataSplit[1].Length - i);
                    }
                }

                var playerPosition = "";
                if (nameData.Contains("FW") && nameData.Contains("AM") || nameData.Contains("FW"))
                    playerPosition = "F";
                else if (nameData.Contains("D"))
                    playerPosition = "D";
                else if (nameData.Contains("GK"))
                    playerPosition = "Gk";
                if (nameData.Contains("M") || nameData.Contains("AM"))
                    playerPosition = "M";
                

                var playerName = nameDataSplit[1];
                var playerRating = list[2].ToString();
                var playerTeam = list[0].ToString();

                var playerDetails = new string[3];
                playerDetails[0] = playerTeam;
                playerDetails[1] = playerRating;
                playerDetails[2] = playerPosition;

                if (!PlayerPricesMap.ContainsKey(playerName))
                    PlayerPricesMap.Add(playerName, playerDetails);
                else
                    Debug.LogError("playerPricesMap already contains key: " + playerName);
            }
        }

        public static void InitializePlayerTransferList(Dictionary<string, string[]> playerPricesMap)
        {
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
                var playerDetails = entryObject.GetComponent<FootballPlayerDetails>();
                playerDetails.playerName = pair.Key;
                playerDetails.team = pair.Value[0];
                playerDetails.rating = pair.Value[1];
                playerDetails.price = pair.Value[1];
                playerDetails.position = pair.Value[2];
                
                if (!TransferList.PlayerTransferEntryMap.ContainsKey(pair.Key))
                    TransferList.PlayerTransferEntryMap.Add(pair.Key, entryObject);
                else
                    Debug.LogError("PlayerTransferEntryMap already contains player name");
                
                playerNameObj.GetComponent<TMP_Text>().text = pair.Key;
                playerPriceObj.GetComponent<TMP_Text>().text = "$" + pair.Value[1];
                playerPositionObj.GetComponent<TMP_Text>().text = pair.Value[2];

                // team logos!!!
                var playersTeamLogo = teamLogos.Find(x => x.name == pair.Value[0]);
                if (playersTeamLogo != null)
                    playerTeamImageObj.GetComponent<Image>().sprite = playersTeamLogo;

                entryObject.gameObject.GetComponent<Canvas>().enabled = true;
                entryObject.gameObject.GetComponent<CanvasScaler>().enabled = true;
                entryObject.gameObject.GetComponent<GraphicRaycaster>().enabled = true;
                entryButton.gameObject.GetComponent<Image>().enabled = true;
                entryButton.gameObject.GetComponent<Button>().enabled = true;
                entryObject.gameObject.SetActive(true);
            }
        }
    }
}