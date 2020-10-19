using System;
using DefaultNamespace;
using TMPro;
using UnityEngine;

namespace ErrorManagement
{
    public class ErrorLogger : MonoBehaviour
    {
        public static ErrorLogger Instance;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
        }
    }
}