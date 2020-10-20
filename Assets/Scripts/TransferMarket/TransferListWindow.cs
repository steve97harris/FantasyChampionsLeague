using System;
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
        public static TransferListWindow Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        /// <summary>
        /// Reads all player information from list of strings and stores them in PlayerRemoteKeyMap.
        /// </summary>
        /// <param name="playerDatabaseCsv"></param>
        public void GetPlayerTransferList(List<string> playerDatabaseCsv)
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
                
                var athleteStats = new AthleteStats()
                {
                    PlayerName = playerName,
                    Position = playerPosition,
                    Price = playerPrice,
                    Rating = playerRating,
                    RemoteConfigKey = playerRemoteConfigKey,
                    Team = playerTeam,
                    TotalPoints = playerFclPoints
                };
                
                TeamSheetSaveData.PlayerRemoteKeyMap.Add(playerRemoteConfigKey, athleteStats);
            }
        }

        /// <summary>
        /// Instantiates a button into the transfer list for each player registered in the player database.
        /// </summary>
        public void InitializePlayerList()
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

            var playerRemoteKeyMap = TeamSheetSaveData.PlayerRemoteKeyMap;
            foreach (var pair in playerRemoteKeyMap)        
            {
                // instantiate new player transfer entry
                var entryObject = Instantiate(playerTransferEntry, transferListContent);
                var entryButton = entryObject.transform.GetChild(0).transform;
                var playerNameObj = entryButton.GetChild(0).gameObject;
                var playerPriceObj = entryButton.GetChild(1).gameObject;
                var playerTeamImageObj = entryButton.GetChild(2).gameObject;
                var playerPositionObj = entryButton.GetChild(3).gameObject;
                
                // set football player details component
                entryObject.AddComponent<FootballPlayerDetails>();
                TeamSheetDatabase.Instance.SetFootballPlayerDetails(entryObject, pair.Value);
                
                playerNameObj.GetComponent<TMP_Text>().text = pair.Value.PlayerName;
                playerPriceObj.GetComponent<TMP_Text>().text = "$" + pair.Value.Price;
                playerPositionObj.GetComponent<TMP_Text>().text = pair.Value.Position;

                // team logos!!!
                var playersTeamLogo = teamLogos.Find(x => x.name == pair.Value.Team);
                if (playersTeamLogo != null)
                    playerTeamImageObj.GetComponent<Image>().sprite = playersTeamLogo;
            }

            DashBoardManager.Instance.SetGameObjectActive(false, "ScreenSelector");
        }

        /// <summary>
        /// Transfer List back button leading to transfer team sheet
        /// </summary>
        public void BackButton_ToTransferTeamSheet()
        {
            DestroyTransferList();

            var transferTeamSheetObj = GameObjectFinder.FindSingleObjectByName("TransferTeamSheet(Clone)");
            if (transferTeamSheetObj == null)
            {
                var teamSheetSaveData =  PlayFabEntityFileManager.Instance.GetTeamSheetData();
                TransferListEntry.Instance.InstantiateTeamSheet("Transfer");
                TeamSheetDatabase.Instance.SetTeamSheetUi(teamSheetSaveData, "TransferTeamSheet(Clone)");
            }
            else
                transferTeamSheetObj.SetActive(true);

            DashBoardManager.Instance.SetGameObjectActive(true, "ScreenSelector");
        }

        /// <summary>
        /// Destroys Transfer List obj 
        /// </summary>
        public void DestroyTransferList()
        {
            var list = GameObjectFinder.FindSingleObjectByName("TransferList(Clone)");
            DestroyImmediate(list.gameObject,true);
        }
    }
}