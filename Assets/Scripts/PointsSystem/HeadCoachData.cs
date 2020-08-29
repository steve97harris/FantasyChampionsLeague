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
        // private string JsonPath => Path.Combine(Application.persistentDataPath, "HeadCoachData.json");

        // public void UpdateHeadCoachSaveData()
        // {
        //     // get TeamSheetSaveData from json
        //     var savedHeadCoachData = GetSavedHeadCoachData() ?? new HeadCoachSaveData();
        //
        //     // update saved data
        //     savedHeadCoachData.CoachName = coachName;
        //     savedHeadCoachData.ClubName = clubName;
        //     savedHeadCoachData.CoachTotalPoints = coachTotalPoints;
        //     savedHeadCoachData.CoachCurrentGwPoints = coachCurrentGwPoints;
        //     savedHeadCoachData.CoachPointsPerGw = coachPointsPerGw;
        //
        //     SaveHeadCoachData(savedHeadCoachData);
        // }

        // public HeadCoachSaveData GetSavedHeadCoachData()
        // {
        //     if (!File.Exists(JsonPath))
        //     {
        //         Debug.LogError("HeadCoachData.json does not exist - creating new one");
        //         
        //         // create json file
        //         File.Create(JsonPath).Dispose();
        //         return new HeadCoachSaveData();
        //     }
        //
        //     using (StreamReader stream = new StreamReader(JsonPath))
        //     {
        //         // convert json string to TeamSheetSaveData
        //         var json = stream.ReadToEnd();
        //         var headCoachSaveData = JsonConvert.DeserializeObject<HeadCoachSaveData>(json);
        //         return headCoachSaveData;
        //     }
        // }

        // public void SaveHeadCoachData(HeadCoachSaveData headCoachSaveData)
        // {
        //     using (StreamWriter stream = new StreamWriter(JsonPath))
        //     {
        //         // convert TeamSheetSaveData to json string
        //         var j = JsonConvert.SerializeObject(headCoachSaveData, Formatting.Indented);
        //         Debug.Log("HeadCoachData Saved (json String): " + j);
        //         
        //         stream.Write(j);
        //     }
        // }
    }
}