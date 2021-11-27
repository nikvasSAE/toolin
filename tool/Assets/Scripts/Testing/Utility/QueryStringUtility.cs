using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public static class QueryStringUtility
{
    
    public static void GetFormattedGraphQLQuery(string queryString, Query GraphQLQuery, string name = "")
    {
        string l_QueryStringName = name ?? GraphQLQuery.name;
        List<Field> QueryFields = GraphQLQuery.fields;
        string output = null, parent = null;
        Field l_PreviousQueryField = null;
        for (int i = 0; i < QueryFields.Count; i++)
        {
            Field l_CurrentQueryField = QueryFields[i];
            if (l_CurrentQueryField.ParentFieldIndexes.Count == 0)
            {
                if (parent == null)
                {
                    int l_IndentationCount = l_CurrentQueryField.ParentFieldIndexes.Count + 2;
                    output += $"\n${IndentQuery(l_IndentationCount)}{l_CurrentQueryField.FieldName}";
                }
                else {
                    int l_IndentationCount = l_PreviousQueryField.ParentFieldIndexes.Count - l_CurrentQueryField.ParentFieldIndexes.Count;
                    while(l_IndentationCount > 0)
                    {
                        output += $"\n{IndentQuery(l_IndentationCount + 1)}}}";
                        l_IndentationCount--;
                    }
                    output += $"\n{IndentQuery(l_CurrentQueryField.ParentFieldIndexes.Count + 2)}{l_CurrentQueryField.FieldName}";
                    parent = null;
                }
                l_PreviousQueryField = l_CurrentQueryField;
                continue;
            }
            if (QueryFields[l_CurrentQueryField.ParentFieldIndexes.Last()].FieldName != parent)
            {
                parent = QueryFields[l_CurrentQueryField.ParentFieldIndexes.Last()].FieldName;
                if (QueryFields[l_CurrentQueryField.ParentFieldIndexes.Last()] == l_PreviousQueryField)
                {
                    output += $"{{\n{IndentQuery(l_CurrentQueryField.ParentFieldIndexes.Count + 2)}{l_CurrentQueryField.FieldName}";
                }
                else
                {
                    int index = l_PreviousQueryField.ParentFieldIndexes.Count - l_CurrentQueryField.ParentFieldIndexes.Count;
                    while(index > 0)
                    {
                        output += $"\n{IndentQuery(index + 1)}}}";
                        index--;
                    }
                    output += $"\n{IndentQuery(l_CurrentQueryField.ParentFieldIndexes.Count + 2)}{l_CurrentQueryField.FieldName}";
                }
                l_PreviousQueryField = l_CurrentQueryField;
            }
            else
            {
                output += $"\n{IndentQuery(l_CurrentQueryField.ParentFieldIndexes.Count + 2)}{l_CurrentQueryField.FieldName}";
                l_PreviousQueryField = l_CurrentQueryField;
            }
            if (i == QueryFields.Count - 1)
            {
                int l_IndentationCount = l_PreviousQueryField.ParentFieldIndexes.Count;
                while(l_IndentationCount > 0)
                {
                    output += $"\n{IndentQuery(l_IndentationCount + 1)}}}";
                    l_IndentationCount--;
                }
            }
        }
        string l_QueryArguments = GetQueryStringArguments(GraphQLQuery);
        string l_QueryPrefix = GetQueryPrefix(GraphQLQuery.type);
        string query = output == null
            ? $"{l_QueryPrefix} {l_QueryStringName}{{\n${IndentQuery(1)}{queryString}{l_QueryArguments}\n}}"
            : $"{l_QueryPrefix} {l_QueryStringName}{{\n${IndentQuery(1)}{queryString}{l_QueryArguments}{{{output}\n{IndentQuery(1)}}}\n}}";
         GraphQLQuery.query = query;
         GraphQLQuery.IsShowingFormattedQuery = true;
    }

    public static bool CheckTypeHasValidSubFields(Introspection.SchemaClass SchemaClass, string queryTypeName)
    {
        Introspection.SchemaClass.Data.Schema.Type queryType = SchemaClass.data.__schema.types.Find(
            (foundType) => foundType.name == queryTypeName
        );
        return queryType?.fields != null && queryType.fields.Count != 0;
    }
    
    private static string GetQueryStringArguments(Query query) 
        => String.IsNullOrEmpty(query.arguments) ? "" : $"({query.arguments})";

    private static string IndentQuery(int amount)
    {
        string output = "";
        for (int i = 0; i < amount; i++)
            output += "    ";
        return output;
    }
    
    private static string GetQueryPrefix(Query.Type queryType)
    {
        switch (queryType)
        {
            case Query.Type.Query:
                return "query";
            case Query.Type.Mutation:
                return "mutation";
            case Query.Type.Subscription:
                return "subscription";
            default:
                return "query";
        }
    }
}