using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace CSV
{
    public class CsvReader : MonoBehaviour
    {
        /// <summary>
        /// Returns list of each line of CSV file
        /// </summary>
        public static List<string> LoadCsvFile(string filePath)
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
    }
}