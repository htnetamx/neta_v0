using System;
using System.Text;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;

namespace Nop.Web.Infrastructure
{
    public class NetaAuronixMessaging
    {
        public static async void Send(string number, string template, params object[] data)
        {
            var url = "https://netamx.calixtachat.com/api/v1/chats?api_token=59cFxxN0bAFnGtRviXp51ac4irjFDv&language=es_MX&";
            using var client = new HttpClient();

            var builder = new StringBuilder();
            builder.Append("channel_id=10&");
            builder.Append($"template_id={template}&");
            builder.Append($"session={number}&");
            foreach(var item in data)
            {
                if(item.GetType().IsArray)
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

            var response = await client.PostAsync(url + builder.ToString().TrimEnd('&'), null);
            string result = await response.Content.ReadAsStringAsync();
        }
    }

    public class MessageRequest
    {

    }
}
