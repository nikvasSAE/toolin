using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ObjectFieldAlignment = Sirenix.Utilities.Editor.ObjectFieldAlignment;
#if UNITY_EDITOR
using UnityEditor;
#if ODIN_INSPECTOR || ODIN_INSPECTOR_3
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Examples;
using Sirenix.Serialization;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

public class GraphQLEditorWindow : OdinEditorWindow
{
    [OnInspectorInit("@m_GraphQLEditorWindowLogo = EditorIcons.OdinInspectorLogo")] [OnInspectorGUI("DrawPreviewLogo", append: true)] [HideLabel] [HideIf("@m_GraphQLEditorWindowLogo != null")]
    private Texture2D m_GraphQLEditorWindowLogo;
   
    [ShowInInspector] [HideLabel] [PreviewField(100, Alignment = Sirenix.OdinInspector.ObjectFieldAlignment.Left)]
    public Texture2D GraphQLEditorWindowLogo => m_GraphQLEditorWindowLogo;
    
    private GraphQLConfig m_GraphQLConnectionSettings;
    private GraphQL client;
    private int m_CurrentQueryIndex;
    
    
    [MenuItem("Window/GraphQL")]
    private static void OpenWindow()
    {
        var window = GetWindow<GraphQLEditorWindow>();
        window.titleContent = new GUIContent("GraphQL Editor");
        window.Show();
    }

    [OnInspectorInit]
    private void OnInspectorInitialized()
    {
        var window = GetWindow<GraphQLEditorWindow>();
        if (window.m_GraphQLConnectionSettings == null)
        {
            window.m_GraphQLConnectionSettings = ExampleHelper.GetScriptableObject<GraphQLConfig>("GraphQL API");
        }
    }

    [OnInspectorGUI]
    private void OnInspectorGUI()
    {
        if (client == null)
        {
            client = new GraphQL(m_GraphQLConnectionSettings.endpoint, m_GraphQLConnectionSettings.authorization);
        }
        
        GraphQLConfig graphConfig = m_GraphQLConnectionSettings;
        SirenixEditorFields.TextField(
            new GUIContent(), 
            graphConfig.name,
            new GUIStyle(SirenixGUIStyles.Label) { fontSize = 18 });
        
        EditorGUILayout.Space();
        client.OnGetUpdatedSchema?.Invoke();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        graphConfig.endpoint = SirenixEditorFields.TextField(new GUIContent("Endpoint"), graphConfig.endpoint, new GUIStyle(SirenixGUIStyles.Label) { fontSize = 10 });

        if (client.IsLoading)
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            SirenixEditorFields.TextField(
                new GUIContent(), 
                "Running an introspection query on the GraphQL API endpoint...", 
                new GUIStyle(SirenixGUIStyles.Label) { fontSize = 10, alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold });
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        if (client.Schema == null)
        {
            return;
        }
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        // ShowQueryFields(client, client.queries, Query.Type.Query);
        // ShowQueryFields(client, client.mutations, Query.Type.Mutation);
        // ShowQueryFields(client, client.subscriptions, Query.Type.Subscription);
        
        EditorUtility.SetDirty(graphConfig);
    }

   //  [Button("Create Query", ButtonSizes.Large, ButtonStyle.Box)]
   // private void CreateNewQuery() => client.OnCreateQueryEvent?.Invoke();
    
    /* 
    [Button("Create Mutation", ButtonSizes.Large, ButtonStyle.Box)]
    private void CreateMutation() => client.OnCreateMutationEvent?.Invoke();
    
    [Button("Create Subscription", ButtonSizes.Large, ButtonStyle.Box)]
    private void CreateSubscription() => client.OnCreateSubscriptionEvent?.Invoke();
    */

    private void ShowQueryFields(GraphQL client, List<Query> clientQueries, Query.Type queryType)
    {
        if (clientQueries != null)
        {
            if (clientQueries.Count > 0)
            {
                SirenixEditorFields.TextField(queryType.ToString());
            }

            for (int i = 0; i < clientQueries.Count; i++)
            {
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                Query l_CurrentQuery = clientQueries[i];
                l_CurrentQuery.name = SirenixEditorFields.TextField(new GUIContent("Query Name"), l_CurrentQuery.name);
                string[] options = l_CurrentQuery.options.ToArray();
                if (String.IsNullOrEmpty(l_CurrentQuery.ReturnValueType))
                {
                    m_CurrentQueryIndex = SirenixEditorFields.Dropdown(queryType.ToString(), m_CurrentQueryIndex, options);
                    l_CurrentQuery.RawQueryStringValue = options[m_CurrentQueryIndex];
                    SirenixEditorFields.TextField(options[m_CurrentQueryIndex]);
                    GUI.color = Color.green;
                    GUIStyle confirmButtonStyle = EditorStyles.foldout;
                    if (GUILayout.Button($"Confirm {queryType}", confirmButtonStyle))
                    {
                        client.OnSetQueryReturnType?.Invoke(l_CurrentQuery, options[m_CurrentQueryIndex]);
                    }
                    GUI.color = Color.red;
                    if (GUILayout.Button("Delete", EditorStyles.foldout))
                    {
                        client.OnDeleteQuery?.Invoke(clientQueries, i);
                    }
                    GUI.color = Color.white;
                    continue;
                }

                if (l_CurrentQuery.IsShowingFormattedQuery)
                {
                    GUILayout.Label(l_CurrentQuery.query);
                    if (l_CurrentQuery.fields.Count > 0)
                    {
                        if (GUILayout.Button($"Modify {queryType}"))
                        {
                            client.OnEditSelectedQuery?.Invoke(l_CurrentQuery);
                        }
                    }

                    if (GUILayout.Button("Delete Query"))
                    {
                        client.OnDeleteQuery?.Invoke(clientQueries, i);
                    }
                    continue;
                }
                EditorGUILayout.LabelField(l_CurrentQuery.RawQueryStringValue, $"Type: {queryType}");
                if (QueryStringUtility.CheckTypeHasValidSubFields(client.Schema, l_CurrentQuery.ReturnValueType))
                {
                    GUIStyle style = EditorStyles.popup;
                    style.fixedHeight = 24.0f;
                    if (GUILayout.Button("Add Query Field", style))
                    {
                        client.OnSetQueryReturnType?.Invoke(l_CurrentQuery, options[m_CurrentQueryIndex]);
                        client.OnQueryFieldsChanged?.Invoke(l_CurrentQuery, l_CurrentQuery.ReturnValueType, null);
                    }
                }
                foreach (Field queryField in l_CurrentQuery.fields)
                {
                    GUI.color = new Color(0.5f, 1f, 0.5f);
                    string[] fieldOptions = queryField.IntrospectionQueryFields.Select((field => field.name)).ToArray();
                    EditorGUILayout.BeginHorizontal();
                    GUIStyle style = EditorStyles.popup;
                    style.contentOffset = new Vector2(queryField.ParentFieldIndexes.Count * 20, 0);
                    style.fixedHeight = 24.0f;
                    queryField.FieldIndex = EditorGUILayout.Popup(queryField.FieldIndex, fieldOptions, style);
                    GUI.color = Color.white;
                    queryField.CheckSubFields(client.Schema);
                    if (queryField.HasSubFields)
                    {
                        if (GUILayout.Button("Add"))
                        {
                            client.OnQueryFieldsChanged?.Invoke(l_CurrentQuery, queryField.IntrospectionQueryFields[queryField.FieldIndex].type, queryField);
                            break;
                        }
                    }
                    GUI.color = new Color(1, 0, 0);
                    GUIStyle closeButtonStyle = EditorStyles.popup;
                    if (GUILayout.Button("X", closeButtonStyle))
                    {
                        int parent = l_CurrentQuery.fields.FindIndex(field => field == queryField);
                        l_CurrentQuery.fields.RemoveAll(field => field.ParentFieldIndexes.Contains(parent));
                        l_CurrentQuery.fields.Remove(queryField);
                        queryField.HasChangedField = false;
                        break;
                    }
                    EditorGUILayout.EndHorizontal();

                    if (queryField.HasChangedField)
                    {
                        int parentIndex = l_CurrentQuery.fields.FindIndex(field => field == queryField);
                        l_CurrentQuery.fields.RemoveAll(field => field.ParentFieldIndexes.Contains(parentIndex));
                        queryField.HasChangedField = false;
                        break;
                    }
                }
                SirenixEditorGUI.IndentSpace();
                SirenixEditorGUI.IndentSpace();
                EditorGUILayout.Space();
                EditorGUILayout.Space();
                if (l_CurrentQuery.fields.Count > 0)
                {
                    GUIStyle style = EditorStyles.popup;
                    GUI.color = new Color(0,1,1);
                    if (GUILayout.Button($"Format {queryType}", style))
                    {
                        QueryStringUtility.GetFormattedGraphQLQuery(l_CurrentQuery.query, l_CurrentQuery);
                    }
                }
                GUI.color = Color.red;
                GUIStyle deleteStyle = EditorStyles.popup;
                if (GUILayout.Button("Delete", deleteStyle))
                {
                    client.OnDeleteQuery?.Invoke(clientQueries, i);
                }
                GUI.color = Color.white;
            }
            SirenixEditorGUI.IndentSpace();
            EditorGUILayout.Space();
        }
    }
    

    [Button("Reset Configuration", ButtonSizes.Large, ButtonStyle.Box), GUIColor(0.5f,0.5f,0.5f,1)]
    private void ResetDatabase()
    {
        client.OnResetQueries?.Invoke();
    }

    [Button("Introspection Query", ButtonSizes.Large, ButtonStyle.Box), GUIColor(0, 0.8f, 0.8f)]
    private void Introspection()
    {
        client.OnIntrospectionQuery?.Invoke();
    }

    private void DrawPreviewLogo()
    {
        if (this.GraphQLEditorWindowLogo == null) return;
        GUILayout.BeginVertical(GUI.skin.box);
        GUILayout.Label(this.GraphQLEditorWindowLogo);
        GUILayout.EndVertical();
    }
}
#endif
#endif