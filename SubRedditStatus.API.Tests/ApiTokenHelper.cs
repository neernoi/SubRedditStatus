using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SubRedditStatus.API.Tests
{
    public static class ApiTokenHelper
    {
       public static async Task<string?> GetJWT(string username, string password, HttpClient httpClient)
        {

                var contentType = new MediaTypeWithQualityHeaderValue("application/json");
                httpClient.DefaultRequestHeaders.Accept.Add(contentType);
                // Prepare the request data
                var requestData = new Dictionary<string, string>
            {
                {"username",$"{username}"},
                {"password",$"{password}"}
            };

                // Make a POST request to obtain the access token
                HttpResponseMessage response = await httpClient.PostAsync("http://localhost:5249/api/token", new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<string>(responseContent);
                }

                return null;
        }
    }
}
