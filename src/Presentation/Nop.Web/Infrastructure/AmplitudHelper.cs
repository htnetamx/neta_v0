using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Nop.Web.Infrastructure
{
    public class AmplitudHelper
    {
        public static async Task<string> PostEvent(AmplitudEvent data)
        {
            using (var client = new HttpClient())
            {
                var url = "https://api.eu.amplitude.com/2/httpapi";

                var response = await client.PostAsJsonAsync<AmplitudEvent>(url, data);
                string result = await response.Content.ReadAsStringAsync();
                return result;
            }
        }

        public class EventProperties
        {
            public double load_time { get; set; }
            public string source { get; set; }
            public List<string> dates { get; set; }
        }

        public class UserProperties
        {
            public int age { get; set; }
            public string gender { get; set; }
            public List<string> interests { get; set; }
        }

        public class Groups
        {
            public string team_id { get; set; }
            public List<string> company_name { get; set; }
        }

        public class Event
        {
            public string user_id { get; set; }
            public string device_id { get; set; }
            public string event_type { get; set; }
            public long time { get; set; }
            public EventProperties event_properties { get; set; }
            public UserProperties user_properties { get; set; }
            public Groups groups { get; set; }
            public string app_version { get; set; }
            public string platform { get; set; }
            public string os_name { get; set; }
            public string os_version { get; set; }
            public string device_brand { get; set; }
            public string device_manufacturer { get; set; }
            public string device_model { get; set; }
            public string carrier { get; set; }
            public string country { get; set; }
            public string region { get; set; }
            public string city { get; set; }
            public string dma { get; set; }
            public string language { get; set; }
            public double price { get; set; }
            public int quantity { get; set; }
            public double revenue { get; set; }
            public string productId { get; set; }
            public string revenueType { get; set; }
            public double location_lat { get; set; }
            public double location_lng { get; set; }
            public string ip { get; set; }
            public string idfa { get; set; }
            public string idfv { get; set; }
            public string adid { get; set; }
            public string android_id { get; set; }
            public int event_id { get; set; }
            public long session_id { get; set; }
            public string insert_id { get; set; }
        }

        public class AmplitudEvent
        {
            public string api_key { get; set; }
            public List<Event> events { get; set; }
        }

    }


    public static class NewtonsoftHttpClientExtensions
    {
        public static async Task<T> GetFromJsonAsync<T>(this HttpClient httpClient, string uri, JsonSerializerSettings settings = null, CancellationToken cancellationToken = default)
        {
            ThrowIfInvalidParams(httpClient, uri);

            var response = await httpClient.GetAsync(uri, cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonConvert.DeserializeObject<T>(json, settings);
        }

        public static async Task<HttpResponseMessage> PostAsJsonAsync<T>(this HttpClient httpClient, string uri, T value, JsonSerializerSettings settings = null, CancellationToken cancellationToken = default)
        {
            ThrowIfInvalidParams(httpClient, uri);

            if (value is null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            var json = JsonConvert.SerializeObject(value, settings);

            var response = await httpClient.PostAsync(uri, new StringContent(json, Encoding.UTF8, "application/json"), cancellationToken);

            response.EnsureSuccessStatusCode();

            return response;
        }

        private static void ThrowIfInvalidParams(HttpClient httpClient, string uri)
        {
            if (httpClient == null)
            {
                throw new ArgumentNullException(nameof(httpClient));
            }

            if (string.IsNullOrWhiteSpace(uri))
            {
                throw new ArgumentException("Can't be null or empty", nameof(uri));
            }
        }
    }
}
