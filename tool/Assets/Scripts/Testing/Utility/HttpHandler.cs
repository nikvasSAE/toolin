using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;


public static class HttpHandler
{
	public static event System.Action<ResponseData> OnRequestResponse;
	
   public static async Task<UnityWebRequest> PostAsync(string url, string details, string authToken = null){
            string jsonData = JsonConvert.SerializeObject(new{query = details});
            byte[] postData = Encoding.ASCII.GetBytes(jsonData);
            UnityWebRequest request = UnityWebRequest.Post(url, UnityWebRequest.kHttpVerbPOST);
            request.uploadHandler = new UploadHandlerRaw(postData);
            request.SetRequestHeader("Content-Type", "application/json");
            if (!String.IsNullOrEmpty(authToken)) 
                request.SetRequestHeader("Authorization", "Bearer " + authToken);

            try{
                await request.SendWebRequest();
            }
            catch(Exception e){
                Debug.Log("Testing exceptions");
                OnRequestResponse?.Invoke(new ResponseData("", e.Message));
            }
			Debug.Log(request.downloadHandler.text);
            
            OnRequestResponse?.Invoke(new ResponseData(request.downloadHandler.text));
            return request;
        }
}
