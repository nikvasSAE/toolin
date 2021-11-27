using System;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

/// <summary>
///     Responsible for handling the JSON data received from the server. 
/// </summary>
public class ResponseData
{
        /// <summary>
        ///     Function to handle querying the object fields <see cref="GetJSONObject" />
        /// </summary>
        public readonly Func<string, JObject> OnQueryObjectFields = (str) => GetJSONObject(str);
    
        /// <summary>
        ///     The Raw JSON response data string 
        /// </summary>
        public string Raw { get; private set; }
        
        /// <summary>
        ///     The formatted 'prettified' JSON response data as a string 
        /// </summary>
        public string Data { get; private set; }
       
        /// <summary>
        ///     The Exception object if for some reason an error occured 
        /// </summary>
        public string Exception { get; private set; }
        
        /// <summary>
        ///     The JSON response data as a <see cref="JObject"/> 
        /// </summary>
        private static JObject m_JsonObject;
    
        /// <summary>
        ///     Constructor for the response data object 
        /// </summary>
        /// <param name="text">The response data as plain text string</param>
        /// <param name="errorException">The error exception, if there is one</param>
        public ResponseData (string text, string errorException = null) 
        {
            Exception = errorException;
            Raw = text;
            Data = text != null ? FormatResponseData(text) : null;
            m_JsonObject = text != null ? JObject.Parse(text) : null;
        }

        /// <summary>
        ///     Gets the value of the specified key from the JSON response data 
        /// </summary>
        /// <param name="key">The key that you are searching for
        ///     For Instance - "data.createUsers.user.id" would return the value of the key "id" in the JSON response data
        /// </param>
        /// <returns></returns>
        private static JObject GetJSONObject(string key)
        {
            return m_JsonObject?.SelectToken(key) as JObject;
        }

        public T GetValueFromObject<T>(string obj, string key)
        {
            return GetJSONObject(obj).GetValue(key).ToObject<T>();
        }
        
        /// <summary>
        ///     Returns the JSON text as a formatted response string 
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        private string FormatResponseData(string json)
        {
            var l_ParsedJsonObject = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(l_ParsedJsonObject, Formatting.Indented);
        }
}
