using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Storage;
using UnityEngine;

namespace DefaultNamespace
{
    public class FirebaseDataStorage : MonoBehaviour
    {
        public static FirebaseDataStorage Instance;
        
        public const string FirebaseFileStorageUrl = "gs://fantasychampionsleague.appspot.com";

        private StorageReference _storageReference = null;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;

            _storageReference = GetStorageRef(FirebaseFileStorageUrl);
        }

        private StorageReference GetStorageRef(string url)
        {
            var storage = FirebaseStorage.DefaultInstance;
            return storage.GetReferenceFromUrl(url);
        }
        
        public void UploadFromBytes(byte[] bytes, string fileName)
        {
            var footballPlayerPointsDataRef = _storageReference.Child(fileName);

            footballPlayerPointsDataRef.PutBytesAsync(bytes).ContinueWith((Task<StorageMetadata> task) =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    if (task.Exception != null)
                        Debug.LogError(task.Exception.ToString());
                }
                else
                {
                    var metadata = task.Result;
                    Debug.Log("Upload Complete!");
                    Debug.Log(Encoding.Default.GetString(bytes));
                }
            });
        }
        
        public void UploadFromLocalFile(string localFilePath, string fileName)
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
                    Debug.Log("Upload Complete!");
                }
            });
        }

        public async Task<Stream> DownloadFileStreamAsync(string fileName)
        {
            var footballPlayerPointsDataRef = _storageReference.Child(fileName);

            Stream fileStream = null;
            await footballPlayerPointsDataRef.GetStreamAsync(stream =>
            {
                fileStream = stream;
            }, null, CancellationToken.None).ContinueWith(resultTask =>
            {
                if (!resultTask.IsFaulted && !resultTask.IsCanceled)
                {
                    Debug.Log("File Stream Download Complete!");
                }
            });

            return fileStream;
        }

        public byte[] ConvertStringArrayListToByteArray(List<string> footballPlayerPointsList)
        {
            return footballPlayerPointsList.SelectMany(s => Encoding.UTF8.GetBytes(s + Environment.NewLine)).ToArray();
        }

        private void ResetFootballPlayerPointsDatabase()
        {
            var x = CsvReader.LoadCsvFileViaPath(DashBoardManager.Instance.defaultFootballPlayerPointsDatabasePath);
            var y = ConvertStringArrayListToByteArray(x);
            UploadFromBytes(y, DashBoardManager.FileNameB);
        }
    }
}