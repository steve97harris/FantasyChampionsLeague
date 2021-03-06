﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Storage;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DefaultNamespace
{
    public class FirebaseDataStorage : MonoBehaviour
    {
        public static FirebaseDataStorage Instance;

        private const string FirebaseFileStorageUrl = "gs://fantasychampionsleague.appspot.com";

        private static StorageReference _storageReference;
        
        public void OnEnable()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                if (Instance != this)
                {
                    Destroy(this.gameObject);
                }
            }
            DontDestroyOnLoad(this.gameObject.transform.parent.gameObject);
        }

        private void Awake()
        {
            _storageReference = GetStorageRef(FirebaseFileStorageUrl);
        }

        private IEnumerator CreateAnonymousUser()
        {
            var auth = FirebaseAuth.DefaultInstance;
            if (auth.CurrentUser != null)
            {
                Debug.Log("User already signed in; " + auth.CurrentUser);
                yield break;
            }
            
            var signInTask = auth.SignInAnonymouslyAsync();
            yield return new WaitUntil(() => signInTask.IsCompleted);

            if (signInTask.Exception == null)
            {
                Debug.Log("Sign In Successful; " + signInTask.Result);
                yield break;
            }
                
            Debug.LogError("Sign In Failed; " + signInTask.Exception);
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
                    {
                        Debug.LogError(task.Exception.ToString());
                    }
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
            Debug.Log(footballPlayerPointsDataRef);

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
                else
                {
                    Debug.LogError(resultTask.Exception);
                }
            });

            return fileStream;
        }

        public async Task<StorageMetadata> DownloadMetaData(string fileName)
        {
            var footballPlayerPointsDataRef = _storageReference.Child(fileName);

            StorageMetadata metadata = null;
            await footballPlayerPointsDataRef.GetMetadataAsync().ContinueWith(task =>
            {
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    Debug.Log("File MetaData Download Complete!" + Environment.NewLine + task.Result.GetCustomMetadata("date"));
                    metadata = task.Result;
                }
                else
                {
                    Debug.LogError(task.Exception);
                }
            });

            return metadata;
        }
        
        public void UploadMetaData(string fileName, string date)
        {
            var footballPlayerPointsDataRef = _storageReference.Child(fileName);
            
            var newMetaData = new MetadataChange
            {
                CustomMetadata = new Dictionary<string, string>
                {
                    { "date", date }
                }
            };

            footballPlayerPointsDataRef.UpdateMetadataAsync(newMetaData).ContinueWith(task =>
            {
                if (!task.IsFaulted && !task.IsCanceled)
                {
                    Debug.Log("File MetaData Upload Complete!" + Environment.NewLine + task.Result.GetCustomMetadata("date"));
                }
                else
                {
                    Debug.LogError(task.Exception);
                }
            });

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