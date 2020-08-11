using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dashboard
{
    [Serializable]
    public class FootballPlayerDetails : MonoBehaviour
    {
        public string playerName;
        public string team;
        public string rating;
        public string price;
        public string position;
        public string teamSheetPosition;
    }
}