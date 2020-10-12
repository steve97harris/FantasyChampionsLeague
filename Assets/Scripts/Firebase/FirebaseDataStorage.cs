using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Storage;
using UnityEngine;

namespace DefaultNamespace
{
    public class FirebaseDataStorage : MonoBehaviour
    {
        public static FirebaseDataStorage Instance;
        
        public const string AppFileStorageUrl = "gs://fantasychampionsleague.appspot.com";

        private StorageReference _storageReference = null;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            _storageReference = GetStorageRef(AppFileStorageUrl);
        }

        private void Start()
        {
            var defaultFclPointsDatabasePath = DashBoardManager.Instance.defaultFootballPlayerPointsDatabasePath;
            var fileName = "FootballPlayerPointsDatabase.csv";
            
            UploadLocalFile(defaultFclPointsDatabasePath, fileName);
            
            var metadata = GetMetaData(fileName);
            var x = metadata.GetCustomMetadata("Jeff");
            if (x == null)
            {
                
            }
        }

        public StorageReference GetStorageRef(string url)
        {
            var storage = FirebaseStorage.DefaultInstance;
            return storage.GetReferenceFromUrl(url);
        }

        public void UploadLocalFile(string localFilePath, string fileName)
        {
            var footballPlayerPointsDataRef = _storageReference.Child(fileName);

            footballPlayerPointsDataRef.PutFileAsync(localFilePath).ContinueWith((Task<StorageMetadata> task) =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    if (task.Exception != null)
                        Debug.LogError(task.Exception.ToString());
                }
                else
                {
                    var metadata = task.Result;
                    var downloadUri = metadata.Reference.GetDownloadUrlAsync();
                    Debug.Log("DownloadUrl: " + downloadUri);
                }
            });
        }

        public void DownloadFromUrl(string fileName)
        {
            var footballPlayerPointsDataRef = _storageReference.Child(fileName);

            footballPlayerPointsDataRef.GetDownloadUrlAsync().ContinueWith(task =>
            {
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    var result = task.Result;
                    Debug.Log(result);
                }
                else
                {
                    Debug.LogError(task.Exception);
                }
            });
        }

        public StorageMetadata GetMetaData(string fileName)
        {
            var footballPlayerPointsDataRef = _storageReference.Child(fileName);

            StorageMetadata metaData = null;
            footballPlayerPointsDataRef.GetMetadataAsync().ContinueWith((Task<StorageMetadata> task) => {
                if (!task.IsFaulted && !task.IsCanceled) 
                {
                    metaData = task.Result;
                    Debug.LogError("CacheControl: " + metaData.CacheControl);
                    Debug.LogError("Bucket: " + metaData.Bucket);
                    Debug.LogError("CustomMetaDataKeys: " + metaData.CustomMetadataKeys);
                    
                    var x = metaData.CustomMetadataKeys.ToList();
                    Debug.LogError(x.Count);
                    for (int i = 0; i < x.Count; i++)
                    {
                        Debug.LogError(x[i]);
                    }
                }
                else
                {
                    Debug.LogError(task.Exception);
                }
            });

            return metaData;
        }

        public void UpdateCustomMetaData(string fileName, Dictionary<string, string> newCustomMetadata)
        {
            var newMetadata = new MetadataChange
            { 
                CustomMetadata = newCustomMetadata
            };

            var footballPlayerPointsDataRef = _storageReference.Child(fileName);
            footballPlayerPointsDataRef.UpdateMetadataAsync(newMetadata).ContinueWith(task =>
            {
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    Firebase.Storage.StorageMetadata meta = task.Result;
                    // meta.ContentType should be an empty string now
                    Debug.Log("MetadataUpdated: " + meta);
                }
                else
                {
                    Debug.LogError(task.Exception);
                }
            });
        }
    }
}