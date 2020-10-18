using System;
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

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            
            _storageReference = GetStorageRef(FirebaseFileStorageUrl);

            var scene = SceneManager.GetActiveScene();
            Debug.LogError(scene.name);
            
            if (scene.name == "PointsManagementSystem")
                FootballerPointsManager.Instance.UpdateFirebaseFootballPlayerPointsDatabaseWithCurrentFixtures();
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
            Debug.LogError(footballPlayerPointsDataRef);

            Stream fileStream = null;
            await footballPlayerPointsDataRef.GetStreamAsync(stream =>
            {
                fileStream = stream;
                Debug.Log("fileStream: " + fileStream);
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