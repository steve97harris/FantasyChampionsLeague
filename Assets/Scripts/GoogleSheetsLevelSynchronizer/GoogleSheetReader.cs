using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Google.Apis.Util.Store;
using UnityEngine;
using Object = System.Object;

namespace GoogleSheetsLevelSynchronizer
{
    static class GoogleSheetReader
    {
        // If modifying these scopes, delete your previously saved credentials
        // at ~/.credentials/sheets.googleapis.com-dotnet-quickstart.json
        static string[] Scopes = { SheetsService.Scope.SpreadsheetsReadonly };
        static string ApplicationName = "Google-Sheets-API-Quickstart";

        public static IList<IList<object>> Reader(String spreadsheetId, String range)
        {
            UserCredential credential;

            using (var stream =
                new FileStream(Application.streamingAssetsPath + "/credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Debug.Log("Credential file saved to: " + credPath);
            }

            // Create Google Sheets API service.
            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define request parameters.
            // String spreadsheetId = "1iufkvofC9UcmJS5ld3R72RJZHz2kFd97BYR-1kL8XeM";
            // String range = "A1:E173";
            
            SpreadsheetsResource.ValuesResource.GetRequest request =
                    service.Spreadsheets.Values.Get(spreadsheetId, range);

            // Prints the names and majors of students in a sample spreadsheet:
            // https://docs.google.com/spreadsheets/d/1BxiMVs0XRA5nFMdKvBdBZjgmUUqptlbs74OgvE2upms/edit
            ValueRange response = request.Execute();
            IList<IList<object>> values = response.Values;
            
            if (values != null && values.Count > 0)
            {
                Debug.Log("Data Found!! :)");
            }
            else
            {
                Debug.LogError("No data found.");
            }
            // Console.Read();

            return values;
        }
    }
}