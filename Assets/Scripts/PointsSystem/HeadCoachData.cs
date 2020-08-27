using System;
using System.IO;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    public class HeadCoachData : MonoBehaviour
    {
        public string coachName;
        public string clubName;
        public int coachTotalPoints;
        public int coachCurrentGwPoints;
        public int[] coachPointsPerGw;

        private string JsonPath => Path.Combine(Application.persistentDataPath, "HeadCoachData.json");

        public void UpdateHeadCoachSaveData()
        {
            // get TeamSheetSaveData from json
            var savedHeadCoachData = GetSavedHeadCoachData() ?? new HeadCoachSaveData();

            // update saved data
            savedHeadCoachData.CoachName = coachName;
            savedHeadCoachData.ClubName = clubName;
            savedHeadCoachData.CoachTotalPoints = coachTotalPoints;
            savedHeadCoachData.CoachCurrentGwPoints = coachCurrentGwPoints;
            savedHeadCoachData.CoachPointsPerGw = coachPointsPerGw;

            SaveTeamSheet(savedHeadCoachData);
        }

        public void SetHeadCoachUi()
        {
            var totalCoachPoints = GameObjectFinder.FindSingleObjectByName("HeadCoachTotalPoints");
            var currentGwPoints = GameObjectFinder.FindSingleObjectByName("CurrentGameweekPoints");

            totalCoachPoints.GetComponent<TMP_Text>().text = coachTotalPoints.ToString();
            currentGwPoints.GetComponent<TMP_Text>().text = coachCurrentGwPoints.ToString();
            
            Debug.LogError("Coach Ui Data: { totalPoints: " + coachTotalPoints + " gwPoints: " + coachCurrentGwPoints + "}");
        }
        
        public HeadCoachSaveData GetSavedHeadCoachData()
        {
            if (!File.Exists(JsonPath))
            {
                Debug.LogError("HeadCoachData.json does not exist - creating new one");
                
                // create json file
                File.Create(JsonPath).Dispose();
                return new HeadCoachSaveData();
            }

            using (StreamReader stream = new StreamReader(JsonPath))
            {
                // convert json string to TeamSheetSaveData
                var json = stream.ReadToEnd();
                var headCoachSaveData = JsonConvert.DeserializeObject<HeadCoachSaveData>(json);
                return headCoachSaveData;
            }
        }

        private void SaveTeamSheet(HeadCoachSaveData headCoachSaveData)
        {
            using (StreamWriter stream = new StreamWriter(JsonPath))
            {
                Debug.LogError("hello");
                // convert TeamSheetSaveData to json string
                var j = JsonConvert.SerializeObject(headCoachSaveData, Formatting.Indented);
                Debug.Log("HeadCoachData (json String): " + j);
                
                stream.Write(j);
            }
        }
    }
}