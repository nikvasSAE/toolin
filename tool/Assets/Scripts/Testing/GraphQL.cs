using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using UnityEngine.Networking;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEditor;
using UnityEngine;

public class GraphQL {
    
    private string endpoint;
    private string token;
    
    private UnityWebRequest m_OutgoingWebRequest;
    private string m_IntrospectionQueryString;
    private Introspection.SchemaClass m_SchemaClass;
    private string m_QueryEndpoint, m_MutationEndpoint, m_SubscriptionEndpoint;
    [SerializeField] private List<Query> m_APIQueries = new List<Query>();
    [SerializeField] private List<Query> m_APIMutations = new List<Query>();
    [SerializeField] private List<Query> m_APISubscriptions = new List<Query>();
    
    public System.Action<string, object, System.Action<ResponseData>, string> OnSendQueryRequest;
    public System.Action<Query, string> OnSetQueryReturnType;
    public readonly System.Action<Query, string, Field?> OnQueryFieldsChanged;
    public System.Action<Query> OnEditSelectedQuery;
    public System.Action<List<Query>, int> OnDeleteQuery;
    public readonly System.Action OnIntrospectionQuery;
    public readonly System.Action OnGetUpdatedSchema;
    public System.Action OnResetQueries;

    public System.Action OnCreateQueryEvent;
    public System.Action OnCreateMutationEvent;
    public System.Action OnCreateSubscriptionEvent;
    
    public bool IsLoading { get; private set; }

    public List<Query> queries {
        get => m_APIQueries;
        set => m_APIQueries = value;
    }
    public List<Query> mutations {
        get => m_APIMutations;
        set => m_APIMutations = value;
    }
    
    public List<Query> subscriptions {
        get => m_APISubscriptions;
        set => m_APISubscriptions = value;
    }

    public Introspection.SchemaClass Schema => m_SchemaClass;
    
    public GraphQL (string Endpoint, string AuthorizationToken = null) 
    {
        this.endpoint = Endpoint;
        this.token = AuthorizationToken;
        OnSendQueryRequest += SendGraphQLQuery;
        OnSetQueryReturnType += SetQueryFieldReturnType;
        OnIntrospectionQuery += IntrospectionQuery;
        OnGetUpdatedSchema += GetUpdatedSchema;
        OnResetQueries += ResetDatabaseQueries;
        OnQueryFieldsChanged += AddFieldToQuery;
        OnEditSelectedQuery += EditSelectedQuery;
        OnDeleteQuery += DeleteQueryAtIndex;
        OnCreateQueryEvent += CreateQuery;
        OnCreateMutationEvent += CreateMutation;
        // OnCreateSubscriptionEvent += CreateSubscription;
    }

    private void SetQueryFieldReturnType(Query query, string fieldReturnType)
    {
        string l_FieldEndpoint;
        switch(query.type)
        {
            case Query.Type.Query: l_FieldEndpoint = m_QueryEndpoint; break;
            case Query.Type.Mutation: l_FieldEndpoint = m_MutationEndpoint; break;
            case Query.Type.Subscription: l_FieldEndpoint = m_SubscriptionEndpoint; break;
            default: l_FieldEndpoint = m_QueryEndpoint; break;
        }
        Introspection.SchemaClass.Data.Schema.Type type = m_SchemaClass.data.__schema.types.Find(x => x.name == l_FieldEndpoint);
        Introspection.SchemaClass.Data.Schema.Type.Field field = type.fields.Find(x => x.name == fieldReturnType);
        query.ReturnValueType = GetFieldTypeForField(field);
        OnSetQueryReturnType -= SetQueryFieldReturnType;
    }

    private string GetFieldTypeForField(Introspection.SchemaClass.Data.Schema.Type.Field forField)
    {
        Field newField = (Field) forField;
        return newField.FieldType;
    }

    private class GraphQLQuery {
        public string query;
        public object variables;
    }

    private UnityWebRequest QueryRequest (string query, object variables, string token = null) {
        var fullQuery = new GraphQLQuery() {
            query = query,
            variables = variables,
        };
        string json = JsonConvert.SerializeObject(fullQuery);
        UnityWebRequest request = UnityWebRequest.Post(endpoint, UnityWebRequest.kHttpVerbPOST);
        byte[] payload = Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(payload);
        request.SetRequestHeader("Content-Type", "application/json");
        if (token != null)
        {
            request.SetRequestHeader("Authorization", "Bearer " + token);
        }
        return request;
    }

    private async Task<UnityWebRequest> SendRequest(string query, string variables = null, System.Action<ResponseData> callback = null, string token = null)
    {
        var request = QueryRequest(query, variables, token);
        try
        {
            await request.SendWebRequest();
        }
        catch(Exception e)
        {
            if (request.result == UnityWebRequest.Result.ConnectionError)
            {
                callback?.Invoke(new ResponseData("", request.error));
            }
        }
        string l_OutputResponse = request.downloadHandler.text;
        Debug.Log(l_OutputResponse);
        var output = new ResponseData(l_OutputResponse);
        callback?.Invoke(output);
        return request;
    }

    private IEnumerator SendRequest (string query, object variables = null, System.Action<ResponseData> callback = null, string token = null) 
    {
        var request = QueryRequest(query, variables, token);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            if (callback != null)
            {
                callback(new ResponseData("", request.error));
                yield break;
            }
        }
        string l_ResponseDataString = request.downloadHandler.text;
        var output = new ResponseData(l_ResponseDataString);
        callback?.Invoke(output);
        request.Dispose();
    }

    private async void IntrospectionQuery()
    {
        IsLoading = true;
        Debug.Log($"Endpoint: {endpoint}");
        m_OutgoingWebRequest = await HttpHandler.PostAsync(endpoint, global::Introspection.schemaIntrospectionQuery, token);
        EditorApplication.update += HandleOnUpdateSchema;
    }

    private void GetUpdatedSchema()
    {
        if (m_SchemaClass == null)
        {
            try
            {
                m_IntrospectionQueryString = File.ReadAllText(Application.dataPath + "/Scripts/Testing/schema.json");
            }
            catch
            {
                return;
            }
            m_SchemaClass = JsonConvert.DeserializeObject<Introspection.SchemaClass>(m_IntrospectionQueryString);
            UpdateIntrospectionEndpoints(m_SchemaClass);
        }
    }

    private void HandleOnUpdateSchema()
    {
        if (!m_OutgoingWebRequest.isDone)
        {
            return; 
        }
        EditorApplication.update -= HandleOnUpdateSchema;
        m_IntrospectionQueryString = m_OutgoingWebRequest.downloadHandler.text;
        IsLoading = false;
        string pretty = PrettyPrintJSONResponse(m_IntrospectionQueryString);
        Debug.Log($"Pretty Schema: {pretty}");
        File.WriteAllText(Application.dataPath + "/Scripts/Testing/schema.json", pretty);
        m_SchemaClass = JsonConvert.DeserializeObject<Introspection.SchemaClass>(m_IntrospectionQueryString);
        UpdateIntrospectionEndpoints(m_SchemaClass);
    }

    private void UpdateIntrospectionEndpoints(Introspection.SchemaClass IntrospectionSchemaClass)
    {
        if (IntrospectionSchemaClass.data.__schema.queryType != null)
            m_QueryEndpoint = IntrospectionSchemaClass.data.__schema.queryType.name;
        if (IntrospectionSchemaClass.data.__schema.mutationType != null)
            m_MutationEndpoint = IntrospectionSchemaClass.data.__schema.mutationType.name;
        if (IntrospectionSchemaClass.data.__schema.subscriptionType != null)
            m_SubscriptionEndpoint = IntrospectionSchemaClass.data.__schema.subscriptionType.name;
    }

    private void ResetDatabaseQueries()
    {
        Debug.Log($"Resetting database queries");
        queries = new List<Query>();
        mutations = new List<Query>();
        subscriptions = new List<Query>();
        OnResetQueries -= ResetDatabaseQueries;
    }

    private void CreateQuery()
    {
        GetUpdatedSchema();
        if (queries == null)
        {
            queries = new List<Query>();
        }
        Query query = new Query
        {
            fields = new List<Field>(),
            options = new List<string>(),
            type = Query.Type.Query
        };
        Introspection.SchemaClass.Data.Schema.Type queryType = m_SchemaClass.data.__schema.types.Find(x => x.name == m_QueryEndpoint);
        foreach (var field in queryType.fields)
            query.options.Add(field.name);
        queries.Add(query);
        OnCreateQueryEvent -= CreateQuery;
    }

    private void CreateMutation()
    {
        GetUpdatedSchema();
        if (mutations == null)
        {
            mutations = new List<Query>();
        }
        Query mutationQuery = new Query
        {
            fields = new List<Field>(),
            options = new List<string>(),
            type = Query.Type.Mutation
        };
        Introspection.SchemaClass.Data.Schema.Type mutationSchemaType = Schema.data.__schema.types.Find(foundType => foundType.name == m_MutationEndpoint);
        if (mutationSchemaType == null)
        {
            Debug.Log($"Could not find any mutations matching the name of: {mutationSchemaType.name}");
            return;
        }
        for (int i = 0; i < mutationSchemaType.fields.Count; i++)
            mutationQuery.options.Add(mutationSchemaType.fields[i].name);
        mutations.Add(mutationQuery);
        OnCreateMutationEvent -= CreateMutation;
    }

    private void CreateSubscription()
    {
        GetUpdatedSchema();
        if (subscriptions == null)
        {
            subscriptions = new List<Query>();
        }
        Query subscriptionQuery = new Query
        {
            fields = new List<Field>(),
            options = new List<string>(),
            type = Query.Type.Subscription
        };
        Introspection.SchemaClass.Data.Schema.Type subscriptionSchemaType = Schema.data.__schema.types.Find(foundType => foundType.name == m_SubscriptionEndpoint);
        if (subscriptionSchemaType == null)
        {
            Debug.Log($"Could not find any subscriptions matching the name of: {subscriptionSchemaType.name}");
            return;
        }
        for (int i = 0; i < subscriptionSchemaType.fields.Count; i++)
            subscriptionQuery.options.Add(subscriptionSchemaType.fields[i].name);
        subscriptions.Add(subscriptionQuery);
        OnCreateSubscriptionEvent -= CreateSubscription;
    }

    private void AddFieldToQuery(Query query, string fieldTypeName, [CanBeNull] Field parent = null)
    {
        Introspection.SchemaClass.Data.Schema.Type currentSchemaFieldType = Schema.data.__schema.types.Find(x => x.name == fieldTypeName);
        List<Introspection.SchemaClass.Data.Schema.Type.Field> l_SubSchemaFieldTypes = currentSchemaFieldType.fields;
        int l_CurrentParentFieldIndex = query.fields.FindIndex(field => field == parent);
        List<int> l_ParentFieldTypeIndexes = new List<int>();
        if (parent != null)
        {
            l_ParentFieldTypeIndexes = new List<int>(parent.ParentFieldIndexes) { l_CurrentParentFieldIndex };
        }
        Field l_CurrentField = new Field
        {
            ParentFieldIndexes = l_ParentFieldTypeIndexes
        };
        foreach (Introspection.SchemaClass.Data.Schema.Type.Field field in l_SubSchemaFieldTypes)
        {
            l_CurrentField.IntrospectionQueryFields.Add((Field)field);
        }
        if (l_CurrentField.ParentFieldIndexes.Count == 0)
        {
            query.fields.Add(l_CurrentField);
        }
        else
        {
            var currentIndex = query.fields.FindLastIndex(field =>
                field.ParentFieldIndexes.Count > l_CurrentField.ParentFieldIndexes.Count &&
                field.ParentFieldIndexes.Contains(l_CurrentField.ParentFieldIndexes.Last()));
            if (currentIndex == -1)
            {
                currentIndex = query.fields.FindLastIndex(field =>
                    field.ParentFieldIndexes.Count > l_CurrentField.ParentFieldIndexes.Count &&
                    field.ParentFieldIndexes.Last() == l_CurrentField.ParentFieldIndexes.Last());
            }
            if (currentIndex == -1)
            {
                currentIndex = l_CurrentField.ParentFieldIndexes.Last();
            }
            currentIndex++;
            query.fields[l_CurrentParentFieldIndex].HasChangedField = false;
            query.fields.Insert(currentIndex, l_CurrentField);
        }
    }
    
    private void EditSelectedQuery(Query queryToEdit)
    {
        queryToEdit.IsShowingFormattedQuery = false;
        queryToEdit.query = "";
        OnEditSelectedQuery -= EditSelectedQuery;
    }

    private void DeleteQueryAtIndex(List<Query> QueriesList, int DeleteIndex)
    {
        QueriesList.RemoveAt(DeleteIndex);
        OnDeleteQuery -= DeleteQueryAtIndex;
    }
    
    private void SendGraphQLQuery (string query, object variables = null, System.Action<ResponseData> callback = null, string AuthorizationToken = "") 
    {
        Coroutiner.CreateInstance(SendRequest(query, variables, callback, AuthorizationToken));
        OnSendQueryRequest -= SendGraphQLQuery;
    }
    
    /// <summary>
    ///     Formats a json string into a pretty printed string.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    private string PrettyPrintJSONResponse(string json)
    {
        var l_ParsedJsonObject = JsonConvert.DeserializeObject(json);
        return JsonConvert.SerializeObject(l_ParsedJsonObject, Formatting.Indented);
    }
}