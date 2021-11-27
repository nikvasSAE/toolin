using System.Collections;
using System.Collections.Generic;


public class Query
{

    #region Private 
    
    private string m_QueryName;
    private string m_QueryArguments;
    private Type m_QueryType = Type.Query;
    private List<Field> m_QueryFields;
    private List<string> m_QueryFieldOptions;
    private string m_ReturnValueType;
    private string m_RawQueryStringValue;
    private bool m_IsShowingFormattedQueryString;
    private string m_FormattedQueryString;
    
    #endregion

    #region Getters / Setters
    
    public string arguments { get => m_QueryArguments; set => m_QueryArguments = value; }
    public Type type { get => m_QueryType; set => m_QueryType = value; }
    public List<Field> fields { get => m_QueryFields; set => m_QueryFields = value; }
    
    public List<string> options { get => m_QueryFieldOptions; set => m_QueryFieldOptions = value; }
    
    public string name { get => m_QueryName; set => m_QueryName = value; }
    
    public string ReturnValueType { get => m_ReturnValueType; set => m_ReturnValueType = value; }
    
    public string RawQueryStringValue { get => m_RawQueryStringValue; set => m_RawQueryStringValue = value; }
    
    public bool IsShowingFormattedQuery { get => m_IsShowingFormattedQueryString; set => m_IsShowingFormattedQueryString = value; }
    
    public string query { get => m_FormattedQueryString; set => m_FormattedQueryString = value; }
    
    #endregion
    
    
    
    /// <summary>
    ///     The possible values for the "type" field of a query.
    /// </summary>
    public enum Type
    {
        Query,
        Mutation,
        Subscription
    }
}
