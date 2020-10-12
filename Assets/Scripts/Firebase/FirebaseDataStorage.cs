using System;
using System.Threading.Tasks;
using Firebase.Storage;
using UnityEngine;

namespace DefaultNamespace
{
    public class FirebaseDataStorage : MonoBehaviour
    {
        public static FirebaseDataStorage Instance;
        
        public const string AppFileStorageUrl = "gs://fantasychampionsleague.appspot.com";

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }

        private void Start()
        {
            var defaultFclPointsDatabasePath = DashBoardManager.Instance.defaultFootballPlayerPointsDatabasePath;
            var fileName = "FootballPlayerPointsDatabase.csv";
            
            UploadLocalFile(defaultFclPointsDatabasePath, fileName);
        }

        public StorageReference GetStorageRef(string url)
        {
            var storage = FirebaseStorage.DefaultInstance;
            return storage.GetReferenceFromUrl(url);
        }

        public void UploadLocalFile(string localFile, string fileName)
        {
            var storageRef = GetStorageRef(AppFileStorageUrl);
            var reference = storageRef.Child(fileName);

            reference.PutFileAsync(localFile).ContinueWith((Task<StorageMetadata> task) =>
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
    }
}