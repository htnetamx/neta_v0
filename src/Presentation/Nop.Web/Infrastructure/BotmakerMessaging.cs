using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

namespace Nop.Web.Infrastructure
{

    public class BotmakerMessaging
    {
        public class Payload
        {
            public string ChatPlatform { get; set; }
            public string ChatChannelNumber { get; set; }
            public string PlatformContactId { get; set; }
            public string RuleNameOrId { get; set; }
            public Dictionary<string, object> Params { get; set; }
        }
        /// <summary>
        /// Envía mensaje (http post async request) a través de la API de botmake utilizando un template para cold messaging
        /// </summary>
        /// <param name="ChatChannelNumber"></param> Whatsapp enabled number
        /// <param name="PlatformContactId"></param> Destinanation number
        /// <param name="RuleNameOrIdp"></param> Botmaker template name
        /// <param name="data"></param> params objetc
        public static async Task<string> Send(string chatChannelNumber, string platformContactId, string ruleNameOrId, Dictionary<string, object> data)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri("https://go.botmaker.com/api/v1.0/intent/v2");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("access-token", "eyJhbGciOiJIUzUxMiJ9.eyJidXNpbmVzc0lkIjoibmV0YSIsIm5hbWUiOiJKb3JnZSBHYXJkdcOxbyIsImFwaSI6dHJ1ZSwiaWQiOiJEendwcUthNzVqUnRSYmpHNXY2dE8zYkFoeGUyIiwiZXhwIjoxNzkzNTcyMTEwLCJqdGkiOiJEendwcUthNzVqUnRSYmpHNXY2dE8zYkFoeGUyIn0.WeeDunXgDyAddDtR4J0BVfb_PRpjetZZi5CjNpL6U2P0t8e0RJMbtJCMi7NcqghABoV8vTpnyooscVNbp9qzig");

                // Create a new Paylod
                Payload payload = new Payload
                {
                    ChatPlatform = "whatsapp",
                    ChatChannelNumber = chatChannelNumber,
                    PlatformContactId = platformContactId,
                    RuleNameOrId = ruleNameOrId,
                    Params =  data 
                };

                var response = await client.PostAsJsonAsync("https://go.botmaker.com/api/v1.0/intent/v2", payload);
                string result = await response.Content.ReadAsStringAsync();
                
                return result;
            }
        }
    }

}
