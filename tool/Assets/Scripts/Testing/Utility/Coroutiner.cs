using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR 
using UnityEditor;
#endif
#if ODIN_INSPECTOR || ODIN_INSPECTOR_3
public static class Coroutiner
{
    public static Coroutine CreateInstance(IEnumerator iteration)
    {
        var l_RoutineHandler = new GameObject("coroutiner");
        var l_Routine = l_RoutineHandler.AddComponent(typeof(EditorCoroutine)) as EditorCoroutine;
        return l_Routine != null ? l_Routine.ProcessRoutine(iteration) : null;
    }
}

public sealed class EditorCoroutine : Singleton<EditorCoroutine>
{

    protected override void Awake()
    {
        m_ShouldPersistAcrossScenes = true;
        base.Awake();
        gameObject.hideFlags = HideFlags.HideAndDontSave;
    }

    public Coroutine ProcessRoutine(IEnumerator iteration) => StartCoroutine(DestroyCoroutineOnCompletion(iteration));

    private IEnumerator DestroyCoroutineOnCompletion(IEnumerator iteration)
    {
        yield return StartCoroutine(iteration);
        #if UNITY_EDITOR
            DestroyImmediate(gameObject);
        #elif UUNITY_STANDALONE
            Destroy(gameObject);
        #endif
    }
}
#endif