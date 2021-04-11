using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _Instance;
    public  static T  Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<T>();

                if (_Instance == null)
                {
                    _Instance = new GameObject(string.Empty, typeof(T)).GetComponent<T>();
                }
            }
            return _Instance;
        }
    }
}
