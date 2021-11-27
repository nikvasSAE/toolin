using System;
using System.Collections.Generic;
using UnityEngine;

#if ODIN_INSPECTOR || ODIN_INSPECTOR_3
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif

[InlineEditor]
[CreateAssetMenu(fileName = "GraphQL API", menuName = "Tools/API Settings")]
public class GraphQLConfig : SerializedScriptableObject
{
    public string endpoint = "https://us-central1-bradley-seymour-api.cloudfunctions.net/graphql";
    public string authorization;
}
#endif