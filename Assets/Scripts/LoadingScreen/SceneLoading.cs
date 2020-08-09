using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LoadingScreen
{
    // public class SceneLoading : MonoBehaviour
    // {
    //     [SerializeField] private Image _progressBar;
    //     private void Start()
    //     {
    //         StartCoroutine(LoadAsyncOperation());
    //     }
    //
    //     IEnumerator LoadAsyncOperation()
    //     {
    //         var game = SceneManager.LoadSceneAsync(1);
    //
    //         while (game.progress < 1)
    //         {
    //             _progressBar.fillAmount = game.progress;
    //             yield return new WaitForEndOfFrame();
    //         }
    //     }
    // }
    //
    // public class MainMenu : MonoBehaviour
    // {
    //     private IEnumerator Start()
    //     {
    //         yield return new WaitForSeconds(2.0f);
    //         SceneManager.LoadScene(0);
    //     }
    // }
}