using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DefaultNamespace;
using Newtonsoft.Json;
using UnityEngine;
using PlayFab.Internal;
using UnityEngine.Serialization;

namespace PlayFab
{
    public class PlayFabEntityFileManager : MonoBehaviour
    {
        public static PlayFabEntityFileManager Instance;

        private static string _teamSheetSaveDataJsonString;
        private static string _headCoachSaveDataJsonString;
        private readonly Dictionary<string, string> _entityFileJson = new Dictionary<string, string>();
        public string activeUploadFileName;
        // GlobalFileLock provides a simplistic way to avoid file collisions, specifically designed for this example.
        public int globalFileLock = 0;
        
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        public void LoadPlayFabPlayerFiles()
        {
            if (globalFileLock != 0)
            {
                Debug.Log("This example overly restricts file operations for safety. Careful consideration must be made when doing multiple file operations in parallel to avoid conflict.");
            }
            
            globalFileLock += 1; // Start GetFiles
            var request = new PlayFab.DataModels.GetFilesRequest { Entity = new PlayFab.DataModels.EntityKey { Id = PlayFabController.EntityId, Type = PlayFabController.EntityType } };
            PlayFabDataAPI.GetFiles(request, OnGetFileMeta, PlayFabController.Instance.ErrorCallback);
        }
        
        private void OnGetFileMeta(PlayFab.DataModels.GetFilesResponse result)
        {
            Debug.Log("Loading " + result.Metadata.Count + " files");

            if (result.Metadata.Count < 2 && result.Metadata.Count > -1)
            {
                if (result.Metadata.Count == 0)
                {
                    CreateNewTeamSheetData();
                    CreateNewHeadCoachData();
                }
                else
                {
                    foreach (var pair in result.Metadata)
                    {
                        if (pair.Value.FileName == "TeamSheetData.json")
                        {
                            CreateNewHeadCoachData();
                        }
                        else
                        {
                            CreateNewTeamSheetData();
                        }
                    }
                }
            }

            _entityFileJson.Clear();
            foreach (var eachFilePair in result.Metadata)
            {
                _entityFileJson.Add(eachFilePair.Key, null);
                
                GetActualFile(eachFilePair.Value);
            }
            
            globalFileLock -= 1; // Finish GetFiles
        }

        private void GetActualFile(PlayFab.DataModels.GetFileMetadata fileData)
        {
            globalFileLock += 1; // Start Each SimpleGetCall
            PlayFabHttp.SimpleGetCall(fileData.DownloadUrl,
                result =>
                {
                    _entityFileJson[fileData.FileName] = Encoding.UTF8.GetString(result);
                    Debug.Log("File Name: " + fileData.FileName);

                    switch (fileData.FileName)
                    {
                        case "TeamSheetData.json":
                            _teamSheetSaveDataJsonString = _entityFileJson[fileData.FileName];
                            Debug.Log("PlayFab TeamSheetData: " + _teamSheetSaveDataJsonString);
                            break;
                        case "HeadCoachData.json":
                            _headCoachSaveDataJsonString = _entityFileJson[fileData.FileName];
                            Debug.Log("PlayFab HeadCoachData: " + _headCoachSaveDataJsonString);
                            break;
                    }

                    globalFileLock -= 1;
                }, 
                // Finish Each SimpleGetCall
                error => { Debug.Log(error); }
            );
        }
        
        public TeamSheetSaveData GetTeamSheetData()
        {
            return JsonConvert.DeserializeObject<TeamSheetSaveData>(_teamSheetSaveDataJsonString);
        }
        
        private void CreateNewTeamSheetData()
        {
            var teamSheetSaveData = new TeamSheetSaveData();
            _teamSheetSaveDataJsonString = JsonConvert.SerializeObject(teamSheetSaveData, Formatting.Indented);
            TeamSheetDatabase.Instance.SetTeamSheetUi(teamSheetSaveData, "TransferTeamSheet");
            TeamSheetDatabase.Instance.SetTeamSheetUi(teamSheetSaveData, "PointsTeamSheet");
        }

        public void SavePlayFabTeamSheetData(TeamSheetSaveData teamSheetSaveData)
        {
            var newTeamSheetSaveDataJsonString = JsonConvert.SerializeObject(teamSheetSaveData, Formatting.Indented);

            if (newTeamSheetSaveDataJsonString == _teamSheetSaveDataJsonString) 
                return;
            
            _teamSheetSaveDataJsonString = newTeamSheetSaveDataJsonString;
            UploadFile("TeamSheetData.json");
        }
        
        public HeadCoachSaveData GetHeadCoachData()
        {
            return JsonConvert.DeserializeObject<HeadCoachSaveData>(_headCoachSaveDataJsonString);
        }
        
        private void CreateNewHeadCoachData()
        {
            var headCoachSaveData = new HeadCoachSaveData();
            _headCoachSaveDataJsonString = JsonConvert.SerializeObject(headCoachSaveData, Formatting.Indented);
        }

        public void SavePlayFabHeadCoachData(HeadCoachSaveData headCoachSaveData)
        {
            _headCoachSaveDataJsonString = JsonConvert.SerializeObject(headCoachSaveData, Formatting.Indented);
            UploadFile("HeadCoachData.json");
        }
        
        private void UploadFile(string fileName)
        {
            if (globalFileLock != 0)
                Debug.Log("This example overly restricts file operations for safety. Careful consideration must be made when doing multiple file operations in parallel to avoid conflict.");

            activeUploadFileName = fileName;

            globalFileLock += 1; // Start InitiateFileUploads
            var request = new PlayFab.DataModels.InitiateFileUploadsRequest
            {
                Entity = new PlayFab.DataModels.EntityKey { Id = PlayFabController.EntityId, Type = PlayFabController.EntityType },
                FileNames = new List<string> { activeUploadFileName },
            };
            PlayFabDataAPI.InitiateFileUploads(request, OnInitFileUpload, OnInitFailed);
        }

        private void OnInitFileUpload(PlayFab.DataModels.InitiateFileUploadsResponse response)
        {
            var payloadStr = "";
            switch (activeUploadFileName)
            {
                case "TeamSheetData.json":
                    payloadStr = _teamSheetSaveDataJsonString;
                    break;
                case "HeadCoachData.json":
                    payloadStr = _headCoachSaveDataJsonString;
                    break;
            }

            var payload = Encoding.UTF8.GetBytes(payloadStr);

            globalFileLock += 1; // Start SimplePutCall
            PlayFabHttp.SimplePutCall(response.UploadDetails[0].UploadUrl,
                payload,
                FinalizeUpload,
                Debug.LogError
            );
            globalFileLock -= 1; // Finish InitiateFileUploads
        }
        
        private void OnInitFailed(PlayFabError error)
        {
            if (error.Error == PlayFabErrorCode.EntityFileOperationPending)
            {
                // This is an error you should handle when calling InitiateFileUploads, but your resolution path may vary
                globalFileLock += 1; // Start AbortFileUploads
                var request = new PlayFab.DataModels.AbortFileUploadsRequest
                {
                    Entity = new PlayFab.DataModels.EntityKey { Id = PlayFabController.EntityId, Type = PlayFabController.EntityType },
                    FileNames = new List<string> { activeUploadFileName },
                };
                PlayFabDataAPI.AbortFileUploads(request, (result) => { globalFileLock -= 1; UploadFile(activeUploadFileName); }, PlayFabController.Instance.ErrorCallback); globalFileLock -= 1; // Finish AbortFileUploads
                globalFileLock -= 1; // Failed InitiateFileUploads
            }
            else
                PlayFabController.Instance.ErrorCallback(error);
        }

        private void FinalizeUpload(byte[] data)
        {
            globalFileLock += 1; // Start FinalizeFileUploads
            var request = new PlayFab.DataModels.FinalizeFileUploadsRequest
            {
                Entity = new PlayFab.DataModels.EntityKey { Id = PlayFabController.EntityId, Type = PlayFabController.EntityType },
                FileNames = new List<string> { activeUploadFileName },
            };
            PlayFabDataAPI.FinalizeFileUploads(request, OnUploadSuccess, PlayFabController.Instance.ErrorCallback);
            globalFileLock -= 1; // Finish SimplePutCall
        }
        private void OnUploadSuccess(PlayFab.DataModels.FinalizeFileUploadsResponse result)
        {
            Debug.Log("File upload success: " + activeUploadFileName);
            globalFileLock -= 1; // Finish FinalizeFileUploads
        }
    }
}