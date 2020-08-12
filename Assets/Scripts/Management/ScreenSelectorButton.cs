using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class ScreenSelectorButton : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        public void OnSelect(BaseEventData eventData)
        {
            gameObject.GetComponentInChildren<TMP_Text>().color = Color.white;
        }

        public void OnDeselect(BaseEventData eventData)
        {
            gameObject.GetComponentInChildren<TMP_Text>().color = new Color(0,0,0,255);
        }
    }
}