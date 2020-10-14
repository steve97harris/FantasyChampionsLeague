using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
    public class CsvReader : MonoBehaviour
    {
        public static CsvReader Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        /// <summary>
        /// Returns list of each line of CSV file
        /// </summary>
        public static List<string> LoadCsvFileViaPath(string filePath)
        {
            var reader = new StreamReader(File.OpenRead(filePath));
            List<string> searchList = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                searchList.Add(line);
            }

            return searchList;
        }
        
        public static List<string> LoadCsvFileViaStream(Stream stream)
        {
            var reader = new StreamReader(stream);
            List<string> searchList = new List<string>();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                searchList.Add(line);
            }

            return searchList;
        }
    }
}