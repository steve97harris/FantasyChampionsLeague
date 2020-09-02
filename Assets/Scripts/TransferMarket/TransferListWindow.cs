﻿using System;
using System.Collections.Generic;
using System.Globalization;
using DefaultNamespace;
using PlayFab;
using Unity.RemoteConfig;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Dashboard
{
    public class TransferListWindow : MonoBehaviour
    {
        public static readonly Dictionary<string, AthleteStats> PlayerRemoteKeyMap = new Dictionary<string, AthleteStats>();
        public static readonly Dictionary<string, string[]> PlayerPricesMap = new Dictionary<string, string[]>();        // name, price

        public static void GetPlayerTransferList(List<string> playerDatabaseCsv)
        {
            // span each line of playerList
            foreach (var line in playerDatabaseCsv)
            {
                var playerInformation = line.Split(',');

                var playerTeam = playerInformation[0];
                var playerName = playerInformation[1];
                var playerPosition = playerInformation[2];
                var playerRating = playerInformation[3];
                var playerPrice = playerInformation[4];
                var playerFclPoints = playerInformation[5];
                var playerRemoteConfigKey = playerInformation[6];
                RemoteConfigManager.PlayerRemoteConfigKeysList.Add(playerRemoteConfigKey);
                
                PlayerRemoteKeyMap.Add(playerRemoteConfigKey, new AthleteStats() 
                {
                    PlayerName = playerName,
                    Position = playerPosition,
                    Price = playerPrice,
                    Rating = playerRating,
                    RemoteConfigKey = playerRemoteConfigKey,
                    Team = playerTeam,
                    TotalPoints = playerFclPoints
                });
                

                var playerDetails = new string[]
                {
                    playerTeam,
                    playerRating,
                    playerPosition,
                    playerPrice,
                    playerFclPoints,
                    playerRemoteConfigKey
                };
                
                if (!PlayerPricesMap.ContainsKey(playerName))
                    PlayerPricesMap.Add(playerName, playerDetails);
                else
                    Debug.LogError("playerPricesMap already contains key: " + playerName);
            }
        }

        public static void InitializePlayerList(Dictionary<string, string[]> playerPricesMap)
        {
            // locate gameObjects
            var transferListContent = GameObjectFinder.FindSingleObjectByName("TransferListContent").transform;
            var playerTransferEntry = Resources.Load<GameObject>("Prefabs/TransferPanel/PlayerTransferEntry");
            
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
                    TotalPoints = pair.Value[4],
                    RemoteConfigKey = pair.Value[5]
                });
                
                playerNameObj.GetComponent<TMP_Text>().text = pair.Key;
                playerPriceObj.GetComponent<TMP_Text>().text = "$" + pair.Value[1];
                playerPositionObj.GetComponent<TMP_Text>().text = pair.Value[2];

                // team logos!!!
                var playersTeamLogo = teamLogos.Find(x => x.name == pair.Value[0]);
                if (playersTeamLogo != null)
                    playerTeamImageObj.GetComponent<Image>().sprite = playersTeamLogo;
            }

            DashBoardManager.Instance.SetGameObjectActive(false, "ScreenSelector");
        }

        public void BackButton_ToTransferTeamSheet()
        {
            DestroyTransferList();

            var transferTeamSheetObj = GameObjectFinder.FindSingleObjectByName("TransferTeamSheet(Clone)");
            if (transferTeamSheetObj == null)
                TransferListEntryInstantiateTransferTeamSheet("Transfer");
            else
                transferTeamSheetObj.SetActive(true);

            DashBoardManager.Instance.SetGameObjectActive(true, "ScreenSelector");
        }

        public void TransferListEntryInstantiateTransferTeamSheet(string teamSheetName)
        {
            var teamSavedData =  PlayFabEntityFileManager.Instance.GetTeamSheetData();
            var transferListEntry = gameObject.GetComponent<TransferListEntry>();
            var teamSheetDatabaseObj = GameObjectFinder.FindSingleObjectByName("TeamSheetDatabase");
            var teamDatabase = teamSheetDatabaseObj.GetComponent<TeamSheetDatabase>();

            switch (teamSheetName)
            {
                case "Transfer":
                    transferListEntry.InstantiateTeamSheet(teamSheetName);
                    break;
                case "Points":
                    transferListEntry.InstantiateTeamSheet(teamSheetName);
                    break;
            }
            teamDatabase.SetTeamSheetUi(teamSavedData, teamSheetName + "TeamSheet(Clone)");
        }

        public static void DestroyTransferList()
        {
            var list = GameObjectFinder.FindSingleObjectByName("TransferList(Clone)");
            DestroyImmediate(list.gameObject,true);
        }
    }
}