using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if ODIN_INSPECTOR || ODIN_INSPECTOR_3
using Sirenix.OdinInspector;


public class Singleton<T> : SerializedMonoBehaviour where T : Component 
{

    private static T s_Instance;

    public static T instance
    {
        get
        {
            if (s_Instance == null)
            {
                s_Instance = FindObjectOfType<T>();
                if (s_Instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(T).Name;
                    s_Instance = obj.AddComponent<T>();
                }
            }
            return s_Instance;
        }
    }

    [SerializeField] protected bool m_ShouldPersistAcrossScenes;
    protected virtual void Awake()
    {
        if (s_Instance != null && s_Instance != this)
            Destroy(gameObject);
        s_Instance = this as T;
        if (m_ShouldPersistAcrossScenes)
            DontDestroyOnLoad(s_Instance.gameObject);
    }
}
#endif