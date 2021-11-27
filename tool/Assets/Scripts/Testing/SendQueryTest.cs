using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using TMPro;

[ExecuteInEditMode]
public class SendQueryTest : SerializedMonoBehaviour
{

    [SerializeField] private List<Action> m_AllActions = new List<Action>();
    [SerializeField] private TMP_Text m_QueryTestString;

    private void SetArguments(object InputArguments)
    {
        string json = JsonConvert.SerializeObject(InputArguments, new EnumInputConverter());
        string args = ConvertJSONToArgument(json);
        Debug.Log("Arguments: " + args);
        
    }

    private string ConvertJSONToArgument(string input)
    {
        char[] json = input.ToCharArray();
        List<int> brackets = new List<int>();
        json[json.Length - 1] = ' ';
        for (int i = 0; i < json.Length; i++)
        {
            if (json[i] == '\"')
            {
                if (brackets.Count == 2)
                {
                    brackets = new List<int>();
                }
                brackets.Add(i);
            }
            if (json[i] == ':')
            {
                json[brackets[1]] = ' ';
            }
        }
        string output = new string(json);
        return output;
    }

    [Button("Test Arguments", ButtonSizes.Large, ButtonStyle.Box)]
    private void TestArguments()
    {
        object arguments = new
        {
            input = new
            {
                name = "Hello World",
                age = 15,
                level = new
                {
                    name = "Level 1",
                    level = 1
                }
            }
        };
        SetArguments(arguments);
    }

    [Button("Send Query", ButtonSizes.Large, ButtonStyle.Box)]
    private void SendQuery()
    {
        m_AllActions.Clear();
        string queryString = @"
            query getUsersQuery
            {
                users {
                    created_at
                    updated_at
                    actions{
                        number_of_actions
                        total_actions_left_direction
                        total_actions_right_direction
                        total_actions_upwards_direction
                        total_actions_downwards_direction
                        __typename
                    }
                }
            }
        ";
        
        GraphQL client = new GraphQL("https://us-central1-bradley-seymour-api.cloudfunctions.net/graphql");
        
       /*  // Sending queries? 
        client.OnSendQueryRequest?.Invoke(
                queryString,
                null,
                (response) =>
                {
                   Debug.Log($"API RESPONSE: {response.Raw}");
                   Debug.Log($"API RESPONSE FORMATTED: {response.Data}");
                },
                null
            );
        */
        string mutationString = @"
                mutation CreateAccountMutation($input: LevelSettingsInput!) {
                   createUser(input: $input) {
                user {
                    _id
                    created_at
                    updated_at
                    actions{
                        number_of_actions
                        total_actions_left_direction
                        total_actions_right_direction
                        total_actions_upwards_direction
                        total_actions_downwards_direction
                        __typename
                   }
                }
              } 
            }
        ";

        int[] values = new[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

        dynamic property = new { Property1 = "Value1", Property2 = "Value2" };
        Debug.Log($"Property: {property}");

        // What about mutations? 
        client.OnSendQueryRequest?.Invoke(
                mutationString,
                new
                {
                    input = new {
                        name = "level_10",
                        data = new
                        {
                            column = values,
                            row = 1
                        }
                    }
                },
                (response) =>
                {
                    JObject l_ResponseDataOutput = JObject.Parse(response.Data);
                    // string l_ResponseDataOutput_user = l_ResponseDataOutput["data"]["createUser"]["user"]["_id"].ToString();
                    // JObject l_ActionsObject = (JObject)l_ResponseDataOutput["data"]["createUser"]["user"]["actions"];
                    // Debug.Log($"Actions Object: " + l_ActionsObject);
                    // Debug.Log(l_ResponseDataOutput_user);
                    // JObject l_QueriedActionsObject = response.OnQueryObjectFields?.Invoke("data.createUser.user.actions");
                    
                    int action = response.GetValueFromObject<int>("data.createUser.user.actions", "number_of_actions");
                    Debug.Log($"Action: {action}");
                    
                   
                    /* Action l_NewActionFromQuery = new Action
                    {
                        TotalActions = l_QueriedActionsObject["number_of_actions"].ToObject<int>(),
                        TotalActionsLeft = l_QueriedActionsObject["total_actions_left_direction"].ToObject<int>(),
                        TotalActionsRight = l_QueriedActionsObject["total_actions_right_direction"].ToObject<int>(),
                        TotalActionsUpwards = l_QueriedActionsObject["total_actions_upwards_direction"].ToObject<int>(),
                        TotalActionsDownwards = l_QueriedActionsObject["total_actions_downwards_direction"].ToObject<int>()
                    };
                    */
                    // Debug.Log($"Action: {l_NewActionFromQuery}");
                    // m_AllActions.Add(l_NewActionFromQuery);
                    m_QueryTestString.text = l_ResponseDataOutput.ToString();
                },
                null
            );
    }

    [System.Serializable]
    private class Action
    {
       [SerializeField] private int m_TotalActions, m_TotalActionsLeft, m_TotalActionsRight, m_TotalActionsUpwards, m_TotalActionsDownwards;
       public int TotalActions {
           get => m_TotalActions;
           set => m_TotalActions = value;
       }
       public int TotalActionsLeft {
           get => m_TotalActionsLeft;
           set => m_TotalActionsLeft = value;
       }
       public int TotalActionsRight {
           get => m_TotalActionsRight;
           set => m_TotalActionsRight = value;
       }
       public int TotalActionsUpwards {
           get => m_TotalActionsUpwards;
           set => m_TotalActionsUpwards = value;
       }
       public int TotalActionsDownwards {
           get => m_TotalActionsDownwards;
           set => m_TotalActionsDownwards = value;
       }
    }
}