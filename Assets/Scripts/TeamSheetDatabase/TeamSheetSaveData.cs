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
        // key: teamSheetPosition, value: AthleteStats
        public Dictionary<string, AthleteStats> teamSheetData = new Dictionary<string, AthleteStats>();
        
        /// <summary>
        /// Map storing all players and their information.
        /// <Key> Footballers unique RemoteConfigKey </Key>
        /// <Value> Footballers AthleteStats </Value>
        /// </summary>
        public static readonly Dictionary<string, AthleteStats> PlayerRemoteKeyMap = new Dictionary<string, AthleteStats>();
    }

    // created second football player details as MonoBehaviour methods cannot be serialized
    public class AthleteStats
    {
        public string PlayerName;
        public string Team;
        public string Rating;
        public string Price;
        public string Position;
        public string TotalPoints;
        public string RemoteConfigKey;
    }
}