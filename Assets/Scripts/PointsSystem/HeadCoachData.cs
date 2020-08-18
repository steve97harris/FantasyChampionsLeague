﻿using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    public class HeadCoachData : MonoBehaviour
    {
        public string coachName;
        public string clubName;
        public int coachTotalPoints;
        public int[] coachPointsPerGw;

        private static string JsonPath => $"{Application.persistentDataPath}/HeadCoachData.json";

        public void UpdateHeadCoachSaveData()
        {
            // get TeamSheetSaveData from json
            var savedHeadCoachData = GetSavedHeadCoachData() ?? new HeadCoachSaveData();

            // update saved data
            savedHeadCoachData.CoachName = coachName;
            savedHeadCoachData.ClubName = clubName;
            savedHeadCoachData.CoachTotalPoints = coachTotalPoints;
            savedHeadCoachData.CoachPointsPerGw = coachPointsPerGw;

            SaveTeamSheet(savedHeadCoachData);
        }
        
        private HeadCoachSaveData GetSavedHeadCoachData()
        {
            if (!File.Exists(JsonPath))
            {
                Debug.LogError("TeamSheetData.json does not exist - creating new one");
                
                // create json file
                File.Create(JsonPath).Dispose();
                return new HeadCoachSaveData();
            }

            using (StreamReader stream = new StreamReader(JsonPath))
            {
                // convert json string to TeamSheetSaveData
                var json = stream.ReadToEnd();
                var teamSheetSaveData = JsonConvert.DeserializeObject<HeadCoachSaveData>(json);
                return teamSheetSaveData;
            }
        }

        private void SaveTeamSheet(HeadCoachSaveData headCoachSaveData)
        {
            using (StreamWriter stream = new StreamWriter(JsonPath))
            {
                // convert TeamSheetSaveData to json string
                var j = JsonConvert.SerializeObject(headCoachSaveData, Formatting.Indented);
                Debug.LogError("json String: " + j);
                
                stream.Write(j);
            }
        }
    }
}