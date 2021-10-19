using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Tasks;
using Nop.Services.Catalog;
using Nop.Services.Orders;

namespace Nop.Services.Stores
{
    class EarningStoreTask : IScheduleTask
    {
        #region Fields

        private readonly IStoreService _storeService;
        private readonly IOrderService _orderService;
        #endregion

        #region Ctor

        public EarningStoreTask(IStoreService storeService, IOrderService orderService)
        {
            _storeService = storeService;
            _orderService = orderService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var stores = await _storeService.GetAllStoresAsync();
            foreach(var info in stores)
            {
                if (!string.IsNullOrWhiteSpace(info.CompanyPhoneNumber) && string.Compare(info.CompanyPhoneNumber, "Sin numero") != 0)
                {
                    var orders = await _orderService.GetOrdersByStoreIdsAsync(info.Id);
                    if (orders != null)
                    {
                        var date = DateTime.UtcNow.AddDays(-7).Date;
                        var earnings = orders
                            .Where(v =>
                                v.CreatedOnUtc.Date >= date &&
                                v.CreatedOnUtc.Date < DateTime.UtcNow.Date)
                            .Sum(v => v.OrderTotal);

                        var infoEarning = earnings * 10 / 100;
                        if(infoEarning > 0)
                        {
                            var message = $"Hola {info.CompanyName} esta semana tus ingresos con Neta fueron ${infoEarning.ToString("N2")} ! Continua compartiendo tu liga --> {info.Url} para aumentar tus ingresos";
                            Send_SMS(
                                info.CompanyPhoneNumber,
                                message
                            );
                        }
                    }
                }
            }
        }

        private static async void Send_SMS(string number, string message)
        {
            using (var client = new HttpClient())
            {
                var url = "https://netamx.calixtachat.com/api/v1/chats?";

                var builder = new StringBuilder();
                builder.Append("api_token=59cFxxN0bAFnGtRviXp51ac4irjFDv&");
                builder.Append($"session={number}&");
                builder.Append($"message={message}&");
                builder.Append("channel_id=5");

                var response = await client.PostAsync(url + builder.ToString(), null);
                string result = await response.Content.ReadAsStringAsync();
            }
        }

        private async Task<string> Send(string number, string template, params object[] data)
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
                builder.Append("channel_id=10&");
                builder.Append("language=es_MX");

                var response = await client.PostAsync(url + builder.ToString(), null);
                string result = await response.Content.ReadAsStringAsync();

                return result;
            }
        }

        private async void Blank(string number, string template, params object[] data)
        {

            var a = number;
            var b = template;
            var c = data;
            var d = "done";

        }
        #endregion
    }
}
