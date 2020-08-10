using System.Collections.Generic;
using System.IO;
using Dashboard;
using DefaultNamespace;
using Newtonsoft.Json;
using UnityEngine;

namespace TeamSheetDatabase
{
    public class TeamSheetDatabase : MonoBehaviour
    {
        private static int maxNumberOfEntries = 15;
        private static string JsonTablePath => $"{Application.persistentDataPath}/TeamSheetData.json";

        #region Event Functions

        private void Start()
        {
            var savedOrders = GetSavedOrders();
            
            SaveOrders(savedOrders);
        }

        private void Update()
        {
            // if (OrderWatcher.CurrentOrderUniqueCode == "" || OrderWatcher.CurrentOrderUniqueCode == _previousOrder) 
            //     return;
            //
            // var currentOrderArray = OrderWatcher.CurrentOrderUniqueCode;
            // SetOrderEntryInfo(currentOrderArray);
            //
            // _previousOrder = OrderWatcher.CurrentOrderUniqueCode;
        }

        #endregion

        public static TeamSheetSaveData GetSavedOrders()
        {
            if (!File.Exists(JsonTablePath))
            {
                Debug.LogError("OrderTable does not exist - creating new one");
                
                File.Create(JsonTablePath).Dispose();
                return new TeamSheetSaveData();
            }

            using (StreamReader stream = new StreamReader(JsonTablePath))
            {
                var json = stream.ReadToEnd();
                return JsonUtility.FromJson<TeamSheetSaveData>(json);
            }
        }

        public static void SaveOrders(TeamSheetSaveData teamSheetSaveData)
        {
            using (StreamWriter stream = new StreamWriter(JsonTablePath))
            {
                
                // json string not accepting dictionary :(  

                // var serializeObject = JsonConvert.SerializeObject(teamSheetSaveData);
                var json = JsonUtility.ToJson(teamSheetSaveData, false);
                Debug.LogError("json String: " + json);
                stream.Write(json);
            }
        }
        
        #region New Order Entry Functions

        public static void AddOrderEntry(FootballPlayerDetails orderEntry, string teamSheetPlayerPosition)
        {
            var savedOrders = GetSavedOrders();

            if (savedOrders.TeamSheetData.Count < maxNumberOfEntries)
            {
                if (!savedOrders.TeamSheetData.ContainsKey(teamSheetPlayerPosition))
                {
                    Debug.LogError(savedOrders.TeamSheetData.Count);
                    savedOrders.TeamSheetData.Add(teamSheetPlayerPosition, orderEntry);
                }
                else
                    savedOrders.TeamSheetData[teamSheetPlayerPosition] = orderEntry;
                
                Debug.LogError("TeamSheetSaveData add: " + savedOrders.TeamSheetData[teamSheetPlayerPosition].playerName + ", " + savedOrders.TeamSheetData[teamSheetPlayerPosition].position);
            }

            if (savedOrders.TeamSheetData.Count > maxNumberOfEntries)
            {
                savedOrders.TeamSheetData[teamSheetPlayerPosition] = orderEntry;
                Debug.LogError("Replace player in this position: " + teamSheetPlayerPosition);
            }

            SaveOrders(savedOrders);
        }
        
        #endregion
    }
}