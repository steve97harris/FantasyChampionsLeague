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
        public static void GetPlayerTransferList(IList<IList<object>> googleSheetPlayersList)
        {
            var playerPricesMap = new Dictionary<string, string[]>();        // name, price
            
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
                
                var playerName = nameDataSplit[1];
                var playerRating = list[2].ToString();
                var playerTeam = list[0].ToString();

                var playerDetails = new string[2];
                playerDetails[0] = playerTeam;
                playerDetails[1] = playerRating;

                if (!playerPricesMap.ContainsKey(playerName))
                    playerPricesMap.Add(playerName, playerDetails);
                else
                    Debug.LogError("playerPricesMap already contains key: " + playerName);
                
                InitializePlayerTransferList(playerPricesMap);
            }
        }

        private static void InitializePlayerTransferList(Dictionary<string, string[]> playerPricesMap)
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
                
                // set football players details component
                playerTransferEntry.AddComponent<FootballPlayerDetails>();
                var playerDetails = playerTransferEntry.GetComponent<FootballPlayerDetails>();
                playerDetails.name = pair.Key;
                playerDetails.team = pair.Value[0];
                playerDetails.rating = pair.Value[1];
                playerDetails.price = pair.Value[1];
                
                TransferList.PlayerTransferEntryMap.Add(pair.Key, playerTransferEntry);
                
                playerNameObj.GetComponent<TMP_Text>().text = pair.Key;
                playerPriceObj.GetComponent<TMP_Text>().text = "$" + pair.Value[1];

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