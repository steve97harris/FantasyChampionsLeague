using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.RemoteConfig;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    public class RemoteConfigManager : MonoBehaviour
    {
        public struct userAttributes {}
        
        public struct appAttributes {}
        
        public static readonly Dictionary<string, string> PlayerConfigKeyMap = new Dictionary<string, string>();
        public static readonly List<string> PlayerRemoteConfigKeys = new List<string>();
        
        public void Awake()
        {
            ConfigManager.FetchCompleted += UpdatePlayerPoints;
            ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
        }

        private void UpdatePlayerPoints(ConfigResponse obj)
        {
            var footballPlayerGwPointsMap = new Dictionary<string,int>();
            for (int i = 0; i < PlayerRemoteConfigKeys.Count; i++)
            {
                var gwPoints = ConfigManager.appConfig.GetInt(PlayerRemoteConfigKeys[i]);
                footballPlayerGwPointsMap.Add(PlayerRemoteConfigKeys[i],gwPoints);
            }
            
            var teamSheetDatabaseObj = GameObjectFinder.FindSingleObjectByName("TeamSheetDatabase");
            var teamDatabase = teamSheetDatabaseObj.GetComponent<TeamSheetDatabase>();
            var teamSheetSaveData = teamDatabase.GetSavedTeamSheet();

            var teamSheetDataMap = teamSheetSaveData.teamSheetData;
            foreach (var pair in teamSheetDataMap)
            {
                foreach (var pair2 in footballPlayerGwPointsMap.Where(pair2 => pair2.Key == pair.Value.RemoteConfigKey))
                {
                    pair.Value.TotalPoints = pair2.Value.ToString();
                    Debug.LogError(pair2.Key + ", " + pair2.Value.ToString());
                }
            }
            
            teamSheetSaveData = new TeamSheetSaveData()
            {
                teamSheetData = teamSheetDataMap
            };
            
            teamDatabase.UpdateTeamSheetUi(teamSheetSaveData, "PointsTeamSheet");
        }

        public void FetchFootballPlayerPoints()
        {
            Debug.LogError("fetch");
            ConfigManager.FetchConfigs<userAttributes, appAttributes>(new userAttributes(), new appAttributes());
        }

        private void OnDestroy()
        {
            ConfigManager.FetchCompleted -= UpdatePlayerPoints;
        }
        
        
    }
}