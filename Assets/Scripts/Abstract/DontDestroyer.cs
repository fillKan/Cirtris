using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontDestroyer : MonoBehaviour
{
    [SerializeField] private string _KeyName;

    public string KeyName => _KeyName;

    private void Awake()
    {
        var finds = FindObjectsOfType<DontDestroyer>();

        if (finds.Length > 1)
        {
            for (int i = 0; i < finds.Length; ++i)
            {
                if (Equals(finds[i])) continue;

                if (finds[i].KeyName.Equals(_KeyName))
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }
        // 같은 오브젝트가 씬에 없다면
        DontDestroyOnLoad(gameObject);
    }
}
