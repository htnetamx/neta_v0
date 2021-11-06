using System;
using System.Text;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Collections.Generic;

namespace Nop.Web.Infrastructure
{

    public class BotmakerMessaging
    {
        public string ChatPlatform { get; set; }
        public string ChatChannelNumber { get; set; }
        public string PlatformContactId { get; set; }
        public string RuleNameOrId { get; set; }
        private string accestoken { get; set; }
        //public params Params { get; set; }

        public static async void Send(string ChatChannelNumber, string PlatformContactId, string RuleNameOrId, params object[] data)
        {
            using (var client = new HttpClient())
            {
                var endpoint = "https://go.botmaker.com/api/v1.0/intent/v2";

                var builder = new StringBuilder();
                
                builder.Append($"template_id={RuleNameOrId}&");
                builder.Append("access-token=accestoken&");
                builder.Append($"PlatformContactId={PlatformContactId}&");
                foreach (var item in data)
                {
                    if (item.GetType().IsArray)
                    {
                        var arr = item as string[];
                        foreach (var item1 in arr)
                        {
                            builder.Append($"vars[]={item1}&");
                        }
                    }
                    else
                    {
                        builder.Append($"vars[]={item}&");
                    }
                }


                var response = await client.PostAsync(endpoint + builder.ToString(), null);
                string result = await response.Content.ReadAsStringAsync();
            }
        }
    }

}
