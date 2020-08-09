using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using UnityEngine;

namespace GoogleSheets
{
    static class LevelSynchronizer
    {
        const string spreadsheetId = "2PACX-1vQEPRyulpDQo_4LmbUCw7SdquVq77KjCd4DXvBmDKyCJyiYC1-uf12IHxprPdcqdnDneyKkTNneeott";
        const string sheetNameAndRange = "Sheet1"; //You can also put a cell range here
        const string p12PathFromAsset = "Plugins/fantasychampionsleague-e4085f82654e.p12";

        public static void SyncLevel()
        {
            String serviceAccountEmail = "steve-harris@fantasychampionsleague.iam.gserviceaccount.com";

            var certificate = new X509Certificate2(Application.dataPath + Path.DirectorySeparatorChar + p12PathFromAsset, "notasecret", X509KeyStorageFlags.Exportable);

            ServiceAccountCredential credential = new ServiceAccountCredential(
                new ServiceAccountCredential.Initializer(serviceAccountEmail)
                {
                    Scopes = new[] { SheetsService.Scope.SpreadsheetsReadonly } 
                    /*
                Without this scope, it will :
                GoogleApiException: Google.Apis.Requests.RequestError
                Request had invalid authentication credentials. Expected OAuth 2 access token, login cookie or other valid authentication credential.
                lol..
                */
                }.FromCertificate(certificate));


            var service = new SheetsService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
            });

            SpreadsheetsResource.ValuesResource.GetRequest request = service.Spreadsheets.Values.Get(spreadsheetId, sheetNameAndRange);

            StringBuilder sb = new StringBuilder();

            ValueRange response = request.Execute();
            IList<IList<object>> values = response.Values;
            if (values != null && values.Count > 0)
            {
                foreach (IList<object> row in values)
                {
                    foreach(object cell in row)
                    {
                        sb.Append(cell.ToString() + " ");
                    }
                    //Concat the whole row
                    Debug.Log(sb.ToString());
                    sb.Clear();
                }
            }
            else
            {
                Debug.Log("No data found.");
            }
        }
    }
}