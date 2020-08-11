using System;
using System.Collections.Generic;
using Dashboard;
using UnityEngine;
using UnityEngine.Serialization;

namespace DefaultNamespace
{
    [Serializable]
    public class TeamSheetSaveData
    {
        public Dictionary<string, JsonPlayerDetails> teamSheetData = new Dictionary<string, JsonPlayerDetails>();
    }

    // created second player details as MonoBehaviour methods cannot be serialized
    public class JsonPlayerDetails
    {
        public string PlayerName;
        public string Team;
        public string Rating;
        public string Price;
        public string Position;
        public string TeamSheetPosition;
    }
}