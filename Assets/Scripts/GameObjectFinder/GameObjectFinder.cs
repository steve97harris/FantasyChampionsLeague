using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class GameObjectFinder : MonoBehaviour
    {
        /// <summary>
        /// Returns all objects with the given name
        /// </summary>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public static GameObject[] FindMultipleObjectsByName(string objectName)
        {
            var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == objectName).ToArray();
            return objects;
        }
        
        /// <summary>
        /// Returns first game object found with the given name
        /// </summary>
        /// <param name="objectName"></param>
        /// <returns></returns>
        public static GameObject FindSingleObjectByName(string objectName)
        {
            var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == objectName).ToArray();
            return objects.Length == 0 ? null : objects[0];
        }
    }
}