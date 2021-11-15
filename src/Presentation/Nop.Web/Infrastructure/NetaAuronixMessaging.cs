using System;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nop.Services.Logging;

namespace Nop.Web.Infrastructure
{
    public class NetaAuronixMessaging
    {

        public static async Task<string> Send(string number, string template, int channelId = 10, params object[] data)
        {
            using (var client = new HttpClient())
            {
                var url = "https://netamx.calixtachat.com/api/v1/chats?";

                var builder = new StringBuilder();
                builder.Append($"template_id={template}&");
                builder.Append("api_token=59cFxxN0bAFnGtRviXp51ac4irjFDv&");
                builder.Append($"session={number}&");
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
                builder.Append($"channel_id={channelId}&");
                builder.Append("language=es_MX");

                var response = await client.PostAsync(url + builder.ToString(), null);
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }

        public static async Task<string> Send_SMS(string number, string message, string channel = "5")
        {
            using (var client = new HttpClient())
            {
                var url = "https://netamx.calixtachat.com/api/v1/chats?";

                var builder = new StringBuilder();
                builder.Append("api_token=59cFxxN0bAFnGtRviXp51ac4irjFDv&");
                builder.Append($"session={number}&");
                builder.Append($"message={message}&");
                builder.Append($"channel_id={channel}");

                var response = await client.PostAsync(url + builder.ToString(), null);
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }
    }

    public class MessageRequest
    {

    }
}
