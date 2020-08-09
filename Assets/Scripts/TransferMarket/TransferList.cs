using System.Collections.Generic;
using UnityEngine;

namespace Dashboard
{
    public class TransferList : MonoBehaviour
    {
        public static Dictionary<string, FootballPlayerDetails> PlayerDatabaseMap = new Dictionary<string, FootballPlayerDetails>();
        
        public static Dictionary<string, GameObject> PlayerTransferEntryMap = new Dictionary<string, GameObject>();
    }
}